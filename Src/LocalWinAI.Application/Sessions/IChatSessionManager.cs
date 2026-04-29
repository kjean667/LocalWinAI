using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Application.Sessions;

/// <summary>Orchestrates which chat session is active and handles lifecycle operations.</summary>
public interface IChatSessionManager
{
    /// <summary>The currently active session. Valid after <see cref="InitializeAsync"/> completes.</summary>
    ChatSession ActiveSession { get; }

    /// <summary>Raised on a background thread when a session title is updated after auto-generation.</summary>
    event EventHandler<ChatSession>? SessionTitleUpdated;

    /// <summary>Loads the most-recent session or creates a new one. Must be called once on startup.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns lightweight metadata for all sessions, most-recent first.</summary>
    Task<IReadOnlyList<ChatSessionSummary>> GetSessionsAsync();

    /// <summary>Creates a new empty session, making it the active session.</summary>
    Task<ChatSession> CreateSessionAsync();

    /// <summary>Loads a session by id and makes it the active session.</summary>
    Task SwitchToSessionAsync(Guid id);

    /// <summary>Deletes a session. Switches to the next available session (or a new one).</summary>
    Task DeleteSessionAsync(Guid id);

    /// <summary>Persists the active session and, on the first exchange, triggers background title generation.</summary>
    Task PersistActiveSessionAsync();
}
