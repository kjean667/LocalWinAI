using System.Diagnostics;
using LocalWinAI.Application.Sessions;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Sessions;
using LocalWinAI.Domain.Usage;

namespace LocalWinAI.Application;

/// <summary>Orchestrates chat turns: builds prompts from session history and delegates generation to the language model.</summary>
public sealed class ChatService : IChatService
{
    private readonly ILanguageModelService _languageModel;
    private readonly IUsageTracker _usageTracker;
    private readonly IChatSessionManager _sessionManager;

    public ChatService(ILanguageModelService languageModel, IUsageTracker usageTracker, IChatSessionManager sessionManager)
    {
        _languageModel = languageModel;
        _usageTracker = usageTracker;
        _sessionManager = sessionManager;
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
            var sw = Stopwatch.StartNew();
            response = await _languageModel.GenerateResponseAsync(prompt, cancellationToken);
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
}
