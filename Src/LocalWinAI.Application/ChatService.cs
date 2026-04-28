using System.Diagnostics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;

namespace LocalWinAI.Application;

/// <summary>Orchestrates chat sessions: builds prompts from history and delegates generation to the language model.</summary>
public sealed class ChatService : IChatService
{
    private readonly ILanguageModelService _languageModel;
    private readonly IUsageTracker _usageTracker;
    private readonly List<string> _history = [];

    public ChatService(ILanguageModelService languageModel, IUsageTracker usageTracker)
    {
        _languageModel = languageModel;
        _usageTracker = usageTracker;
    }

    public async Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        _history.Add(userMessage);

        var ready = await _languageModel.EnsureReadyAsync(cancellationToken);
        if (!ready)
            throw new InvalidOperationException("Language model is not ready.");

        var prompt = string.Join("\n", _history);
        var sw = Stopwatch.StartNew();
        var response = await _languageModel.GenerateResponseAsync(prompt, cancellationToken);
        sw.Stop();

        _history.Add(response);

        await _usageTracker.RecordAsync(new UsageEvent(
            DateTimeOffset.UtcNow,
            "chat",
            "chat",
            UsageEvent.EstimateTokens(prompt),
            UsageEvent.EstimateTokens(response),
            (int)sw.ElapsedMilliseconds));

        return response;
    }

    public void ClearConversation() => _history.Clear();
}
