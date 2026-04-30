using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Workspaces;

/// <summary>Single source of truth for the in-memory workspace list with CRUD and change notification.</summary>
public interface IWorkspaceManager
{
    /// <summary>In-memory workspace summaries, ordered by UpdatedAt descending.</summary>
    IReadOnlyList<WorkspaceSummary> Summaries { get; }

    /// <summary>Raised after any create, save, or delete operation.</summary>
    event EventHandler? WorkspacesChanged;

    /// <summary>Populates <see cref="Summaries"/> from the repository. Must be called once on startup.</summary>
    Task LoadAsync(CancellationToken ct = default);

    /// <summary>Returns the full workspace with the given id, or null if not found.</summary>
    Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates a new workspace with the given name, persists it, and raises <see cref="WorkspacesChanged"/>.</summary>
    Task<Workspace> CreateAsync(string name, CancellationToken ct = default);

    /// <summary>Bumps UpdatedAt, persists the workspace, updates Summaries, and raises <see cref="WorkspacesChanged"/>.</summary>
    Task SaveAsync(Workspace workspace, CancellationToken ct = default);

    /// <summary>Deletes the workspace by id, removes it from Summaries, and raises <see cref="WorkspacesChanged"/>.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
