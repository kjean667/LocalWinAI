using LocalWinAI.Application.Sessions;
using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeChatSessionManager : IChatSessionManager
{
    public ChatSession ActiveSession { get; set; } = new();
    public List<ChatSession> AllSessions { get; } = [];
    public int PersistCallCount { get; private set; }

    public event EventHandler<ChatSession>? SessionTitleUpdated;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (AllSessions.Count == 0)
            AllSessions.Add(ActiveSession);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatSessionSummary>> GetSessionsAsync()
        => Task.FromResult<IReadOnlyList<ChatSessionSummary>>(AllSessions
            .Select(s => new ChatSessionSummary(s.Id, s.Title, s.CreatedAt, s.LastUsedAt))
            .ToList());

    public Task<ChatSession> CreateSessionAsync()
    {
        var session = new ChatSession();
        AllSessions.Add(session);
        ActiveSession = session;
        return Task.FromResult(session);
    }

    public Task SwitchToSessionAsync(Guid id)
    {
        var session = AllSessions.FirstOrDefault(s => s.Id == id)
            ?? throw new InvalidOperationException($"Session {id} not found.");
        ActiveSession = session;
        return Task.CompletedTask;
    }

    public Task DeleteSessionAsync(Guid id)
    {
        AllSessions.RemoveAll(s => s.Id == id);
        if (ActiveSession.Id == id)
            ActiveSession = AllSessions.Count > 0 ? AllSessions[^1] : new ChatSession();
        return Task.CompletedTask;
    }

    public Task PersistActiveSessionAsync()
    {
        PersistCallCount++;
        return Task.CompletedTask;
    }

    public void RaiseSessionTitleUpdated(ChatSession session)
        => SessionTitleUpdated?.Invoke(this, session);
}
