using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Infrastructure.Workspaces;

/// <summary>
/// Persists workspaces under <c>%LocalAppData%\LocalWinAI\workspaces\</c>.
/// Each workspace is stored as <c>{id}.json</c>. A lightweight <c>index.json</c> holds
/// <c>{ Id, Name, IconGlyph, AccentColorHex, UpdatedAt }</c> so the sidebar can be populated
/// without loading every full workspace file.
/// </summary>
public sealed class WorkspaceRepository : IWorkspaceRepository
{
    internal static readonly string StorageRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LocalWinAI",
        "workspaces");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<WorkspaceSummary>> GetAllAsync(CancellationToken ct = default)
    {
        var indexPath = IndexPath();
        if (!File.Exists(indexPath))
            return [];

        var json = await File.ReadAllTextAsync(indexPath, ct);
        var index = JsonSerializer.Deserialize<List<WorkspaceIndexEntry>>(json, JsonOptions) ?? [];

        return index
            .OrderByDescending(e => e.UpdatedAt)
            .Select(e => new WorkspaceSummary(e.Id, e.Name, e.IconGlyph, e.AccentColorHex, e.UpdatedAt))
            .ToList();
    }

    public async Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var path = WorkspacePath(id);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<Workspace>(json, JsonOptions);
    }

    public async Task SaveAsync(Workspace workspace, CancellationToken ct = default)
    {
        Directory.CreateDirectory(StorageRoot);
        workspace.UpdatedAt = DateTimeOffset.UtcNow;

        var workspaceJson = JsonSerializer.Serialize(workspace, JsonOptions);
        await WriteWithRetryAsync(WorkspacePath(workspace.Id), workspaceJson, ct);
        await UpdateIndexAsync(workspace, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var path = WorkspacePath(id);
        if (File.Exists(path))
            File.Delete(path);

        var memoryPath = MemoryPath(id);
        if (File.Exists(memoryPath))
            File.Delete(memoryPath);

        var indexPath = IndexPath();
        if (!File.Exists(indexPath))
            return;

        var json = await File.ReadAllTextAsync(indexPath, ct);
        var index = JsonSerializer.Deserialize<List<WorkspaceIndexEntry>>(json, JsonOptions) ?? [];
        index.RemoveAll(e => e.Id == id);
        await WriteWithRetryAsync(indexPath, JsonSerializer.Serialize(index, JsonOptions), ct);
    }

    private async Task UpdateIndexAsync(Workspace workspace, CancellationToken ct)
    {
        var indexPath = IndexPath();
        List<WorkspaceIndexEntry> index = [];

        if (File.Exists(indexPath))
        {
            var existing = await File.ReadAllTextAsync(indexPath, ct);
            index = JsonSerializer.Deserialize<List<WorkspaceIndexEntry>>(existing, JsonOptions) ?? [];
        }

        var i = index.FindIndex(e => e.Id == workspace.Id);
        var entry = new WorkspaceIndexEntry(workspace.Id, workspace.Name, workspace.IconGlyph, workspace.AccentColorHex, workspace.UpdatedAt);
        if (i >= 0)
            index[i] = entry;
        else
            index.Add(entry);

        await WriteWithRetryAsync(indexPath, JsonSerializer.Serialize(index, JsonOptions), ct);
    }

    private static async Task WriteWithRetryAsync(string path, string content, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    path, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 4096, useAsync: true);
                await stream.WriteAsync(bytes, ct);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                await Task.Delay(50, ct);
            }
        }
    }

    private static string WorkspacePath(Guid id) => Path.Combine(StorageRoot, $"{id}.json");
    private static string MemoryPath(Guid id) => Path.Combine(StorageRoot, $"{id}.memory.md");
    private static string IndexPath() => Path.Combine(StorageRoot, "index.json");

    private sealed record WorkspaceIndexEntry(Guid Id, string Name, string IconGlyph, string AccentColorHex, DateTimeOffset UpdatedAt);
}
