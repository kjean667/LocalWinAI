using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeWorkspaceRepository : IWorkspaceRepository
{
    private readonly Dictionary<Guid, Workspace> _store = [];

    public Task<IReadOnlyList<WorkspaceSummary>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<WorkspaceSummary> result = _store.Values
            .OrderByDescending(w => w.UpdatedAt)
            .Select(w => new WorkspaceSummary(w.Id, w.Name, w.IconGlyph, w.AccentColorHex, w.UpdatedAt))
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var workspace);
        return Task.FromResult(workspace);
    }

    public Task SaveAsync(Workspace workspace, CancellationToken ct = default)
    {
        _store[workspace.Id] = workspace;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public int Count => _store.Count;
    public Workspace? this[Guid id] => _store.GetValueOrDefault(id);
}
