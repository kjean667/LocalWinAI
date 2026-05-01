namespace LocalWinAI.Domain.Sessions;

/// <summary>Aggregate root for a persistent chat session and its message history.</summary>
public sealed class ChatSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = "New conversation";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? WorkspaceId { get; set; }

    private readonly List<ChatMessage> _messages = [];
    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void AddMessage(ChatMessage message) => _messages.Add(message);
    public void RemoveLastMessage()
    {
        if (_messages.Count > 0)
            _messages.RemoveAt(_messages.Count - 1);
    }
}
