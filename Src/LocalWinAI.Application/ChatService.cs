using LocalWinAI.Domain;

namespace LocalWinAI.Application;

/// <summary>Orchestrates chat sessions: builds prompts from history and delegates generation to the language model.</summary>
public sealed class ChatService : IChatService
{
    private readonly ILanguageModelService _languageModel;
    private readonly List<string> _history = [];

    public ChatService(ILanguageModelService languageModel)
    {
        _languageModel = languageModel;
    }

    public async Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        _history.Add(userMessage);

        var ready = await _languageModel.EnsureReadyAsync(cancellationToken);
        if (!ready)
            throw new InvalidOperationException("Language model is not ready.");

        var prompt = string.Join("\n", _history);
        var response = await _languageModel.GenerateResponseAsync(prompt, cancellationToken);

        _history.Add(response);
        return response;
    }

    public void ClearConversation() => _history.Clear();
}
