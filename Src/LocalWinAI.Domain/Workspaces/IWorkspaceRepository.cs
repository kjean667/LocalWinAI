namespace LocalWinAI.Domain.Workspaces;

/// <summary>Persistence contract for workspaces.</summary>
public interface IWorkspaceRepository
{
    /// <summary>Returns lightweight workspace metadata for all workspaces, ordered by most-recently updated first.</summary>
    Task<IReadOnlyList<WorkspaceSummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns the full workspace, or null if not found.</summary>
    Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Saves (creates or updates) a workspace.</summary>
    Task SaveAsync(Workspace workspace, CancellationToken ct = default);

    /// <summary>Deletes a workspace by id.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
