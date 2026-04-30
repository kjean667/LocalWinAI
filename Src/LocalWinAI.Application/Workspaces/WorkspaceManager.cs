using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Workspaces;

/// <summary>Implements <see cref="IWorkspaceManager"/>, keeping Summaries in sync with the repository.</summary>
public sealed class WorkspaceManager : IWorkspaceManager
{
    private readonly IWorkspaceRepository _repository;
    private List<WorkspaceSummary> _summaries = [];

    public IReadOnlyList<WorkspaceSummary> Summaries => _summaries;
    public event EventHandler? WorkspacesChanged;

    public WorkspaceManager(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        var all = await _repository.GetAllAsync(ct);
        _summaries = [.. all.OrderByDescending(s => s.UpdatedAt)];
    }

    public Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default)
        => _repository.GetAsync(id, ct);

    public async Task<Workspace> CreateAsync(string name, CancellationToken ct = default)
    {
        var workspace = new Workspace { Name = name };
        await SaveAsync(workspace, ct);
        return workspace;
    }

    public async Task SaveAsync(Workspace workspace, CancellationToken ct = default)
    {
        workspace.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveAsync(workspace, ct);

        var summary = new WorkspaceSummary(workspace.Id, workspace.Name, workspace.IconGlyph, workspace.AccentColorHex, workspace.UpdatedAt);
        var index = _summaries.FindIndex(s => s.Id == workspace.Id);
        if (index >= 0)
            _summaries[index] = summary;
        else
            _summaries.Add(summary);

        _summaries = [.. _summaries.OrderByDescending(s => s.UpdatedAt)];
        WorkspacesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(id, ct);
        _summaries.RemoveAll(s => s.Id == id);
        WorkspacesChanged?.Invoke(this, EventArgs.Empty);
    }
}
