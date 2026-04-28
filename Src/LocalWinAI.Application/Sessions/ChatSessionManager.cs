using LocalWinAI.Domain;
using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Application.Sessions;

/// <summary>Implements <see cref="IChatSessionManager"/>, wrapping the repository and tracking the active session.</summary>
public sealed class ChatSessionManager : IChatSessionManager
{
    private readonly IChatSessionRepository _repository;
    private readonly ILanguageModelService _languageModel;
    // Assigned in InitializeAsync before any caller can reach ActiveSession.
    private ChatSession _activeSession = null!;

    public ChatSession ActiveSession => _activeSession;
    public event EventHandler<ChatSession>? SessionTitleUpdated;

    public ChatSessionManager(IChatSessionRepository repository, ILanguageModelService languageModel)
    {
        _repository = repository;
        _languageModel = languageModel;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync();
        if (all.Count > 0)
        {
            var mostRecent = all.OrderByDescending(s => s.LastUsedAt).First();
            _activeSession = await _repository.GetAsync(mostRecent.Id) ?? new ChatSession();
        }
        else
        {
            _activeSession = await CreateNewAndSaveAsync();
        }
    }

    public Task<IReadOnlyList<ChatSession>> GetSessionsAsync() => _repository.GetAllAsync();

    public async Task<ChatSession> CreateSessionAsync()
    {
        _activeSession = await CreateNewAndSaveAsync();
        return _activeSession;
    }

    public async Task SwitchToSessionAsync(Guid id)
    {
        var session = await _repository.GetAsync(id)
            ?? throw new InvalidOperationException($"Session {id} not found.");
        _activeSession = session;
        _activeSession.LastUsedAt = DateTimeOffset.UtcNow;
        await _repository.SaveAsync(_activeSession);
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        await _repository.DeleteAsync(id);

        if (_activeSession.Id == id)
        {
            var remaining = await _repository.GetAllAsync();
            if (remaining.Count > 0)
            {
                var next = remaining.OrderByDescending(s => s.LastUsedAt).First();
                _activeSession = await _repository.GetAsync(next.Id) ?? new ChatSession();
            }
            else
            {
                _activeSession = await CreateNewAndSaveAsync();
            }
        }
    }

    public async Task PersistActiveSessionAsync()
    {
        _activeSession.LastUsedAt = DateTimeOffset.UtcNow;
        await _repository.SaveAsync(_activeSession);

        // After first exchange (one user + one AI message), generate a title in the background.
        if (_activeSession.Messages.Count == 2 && _activeSession.Title == "New conversation")
        {
            var firstUserMessage = _activeSession.Messages[0].Text;
            _ = GenerateTitleAsync(_activeSession, firstUserMessage);
        }
    }

    private async Task GenerateTitleAsync(ChatSession session, string firstUserMessage)
    {
        try
        {
            var ready = await _languageModel.EnsureReadyAsync();
            string title;
            if (ready)
            {
                var prompt = $"Summarize the following user message as a conversation title in 4 to 6 words: {firstUserMessage}";
                var raw = await _languageModel.GenerateResponseAsync(prompt);
                title = raw.Trim().TrimEnd('.');
                if (string.IsNullOrWhiteSpace(title))
                    title = TruncateTitle(firstUserMessage);
            }
            else
            {
                title = TruncateTitle(firstUserMessage);
            }

            session.Title = title;
        }
        catch (Exception)
        {
            session.Title = TruncateTitle(firstUserMessage);
        }

        await _repository.SaveAsync(session);
        SessionTitleUpdated?.Invoke(this, session);
    }

    private async Task<ChatSession> CreateNewAndSaveAsync()
    {
        var session = new ChatSession();
        await _repository.SaveAsync(session);
        return session;
    }

    private static string TruncateTitle(string message)
        => message.Length <= 50 ? message : message[..50];
}
