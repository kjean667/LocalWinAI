namespace LocalWinAI.Domain.Sessions;

/// <summary>Persistence contract for chat sessions.</summary>
public interface IChatSessionRepository
{
    /// <summary>Returns lightweight session metadata for all sessions (no messages), ordered by most-recent first.</summary>
    Task<IReadOnlyList<ChatSessionSummary>> GetAllAsync();

    /// <summary>Returns the full session including all messages, or null if not found.</summary>
    Task<ChatSession?> GetAsync(Guid id);

    /// <summary>Saves (creates or updates) a session and refreshes the index.</summary>
    Task SaveAsync(ChatSession session);

    /// <summary>Deletes a session and removes it from the index.</summary>
    Task DeleteAsync(Guid id);
}
