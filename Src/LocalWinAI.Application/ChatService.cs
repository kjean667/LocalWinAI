using System.Diagnostics;
using System.Text;
using LocalWinAI.Application.Sessions;
using LocalWinAI.Application.Tools;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Sessions;
using LocalWinAI.Domain.Usage;
using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application;

/// <summary>Orchestrates chat turns: builds prompts from session history and delegates generation to the language model.</summary>
public sealed class ChatService : IChatService
{
    private readonly ILanguageModelService _languageModel;
    private readonly IUsageTracker _usageTracker;
    private readonly IChatSessionManager _sessionManager;
    private readonly IWorkspaceRepository? _workspaceRepository;
    private readonly IToolRegistry? _toolRegistry;
    private const int MaxAgentIterations = 5;

    public ChatService(
        ILanguageModelService languageModel,
        IUsageTracker usageTracker,
        IChatSessionManager sessionManager,
        IWorkspaceRepository? workspaceRepository = null,
        IToolRegistry? toolRegistry = null)
    {
        _languageModel = languageModel;
        _usageTracker = usageTracker;
        _sessionManager = sessionManager;
        _workspaceRepository = workspaceRepository;
        _toolRegistry = toolRegistry;
    }

    public async Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var session = _sessionManager.ActiveSession;
        session.AddMessage(new ChatMessage(ChatMessageSender.User, userMessage, DateTimeOffset.UtcNow));

        string response;
        try
        {
            var ready = await _languageModel.EnsureReadyAsync(cancellationToken);
            if (!ready)
            {
                session.RemoveLastMessage();
                throw new InvalidOperationException("Language model is not ready.");
            }

            var prompt = string.Join("\n", session.Messages.Select(m => m.Text));

            IFileToolContext? toolContext = null;
            if (session.WorkspaceId is { } workspaceId && _workspaceRepository is not null)
            {
                var workspace = await _workspaceRepository.GetAsync(workspaceId, cancellationToken);
                if (workspace is { } ws && ws.Folders.Count > 0)
                    toolContext = new FileToolContext(ws.Folders.ToList());
            }

            var sw = Stopwatch.StartNew();
            response = await RunAgentLoopAsync(prompt, toolContext, cancellationToken);
            sw.Stop();

            session.AddMessage(new ChatMessage(ChatMessageSender.AI, response, DateTimeOffset.UtcNow));

            await _usageTracker.RecordAsync(new UsageEvent(
                DateTimeOffset.UtcNow,
                "chat",
                "chat",
                UsageEvent.EstimateTokens(prompt),
                UsageEvent.EstimateTokens(response),
                (int)sw.ElapsedMilliseconds));

            await _sessionManager.PersistActiveSessionAsync();
        }
        catch (OperationCanceledException)
        {
            session.RemoveLastMessage();
            throw;
        }

        return response;
    }

    private async Task<string> RunAgentLoopAsync(string prompt, IFileToolContext? toolContext, CancellationToken cancellationToken)
    {
        var response = string.Empty;
        for (var iteration = 0; iteration < MaxAgentIterations; iteration++)
        {
            response = await _languageModel.GenerateResponseAsync(prompt, cancellationToken);

            if (_toolRegistry is null || toolContext is null)
                break;

            var toolCalls = ToolCallParser.Parse(response);
            if (toolCalls.Count == 0 || iteration == MaxAgentIterations - 1)
                break;

            var sb = new StringBuilder(prompt);
            sb.Append('\n').Append(response);
            foreach (var call in toolCalls)
            {
                var result = _toolRegistry.TryGetTool(call.Name, out var tool)
                    ? await tool!.ExecuteAsync(toolContext, call.ArgumentsJson, cancellationToken)
                    : $"Unknown tool: {call.Name}";
                sb.Append('\n').Append(ToolResultFormatter.Format(call.Name, result));
            }
            prompt = sb.ToString();
        }
        return response;
    }
}
