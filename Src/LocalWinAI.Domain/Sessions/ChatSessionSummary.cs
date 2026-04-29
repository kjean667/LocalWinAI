namespace LocalWinAI.Domain.Sessions;

/// <summary>Lightweight, message-free snapshot of a session used for sidebar lists and index queries.</summary>
public sealed record ChatSessionSummary(Guid Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset LastUsedAt);
