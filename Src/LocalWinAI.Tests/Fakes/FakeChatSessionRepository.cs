using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeChatSessionRepository : IChatSessionRepository
{
    private readonly Dictionary<Guid, ChatSession> _store = [];

    public Task<IReadOnlyList<ChatSession>> GetAllAsync()
    {
        IReadOnlyList<ChatSession> result = _store.Values
            .Select(s => new ChatSession
            {
                Id = s.Id,
                Title = s.Title,
                CreatedAt = s.CreatedAt,
                LastUsedAt = s.LastUsedAt
            })
            .ToList();
        return Task.FromResult(result);
    }

    public Task<ChatSession?> GetAsync(Guid id)
    {
        _store.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task SaveAsync(ChatSession session)
    {
        _store[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public int Count => _store.Count;
    public ChatSession? this[Guid id] => _store.GetValueOrDefault(id);
}
