namespace LocalWinAI.Domain.Sessions;

/// <summary>An immutable record of a single message in a persistent chat session.</summary>
public sealed record ChatMessage(ChatMessageSender Sender, string Text, DateTimeOffset Timestamp);
