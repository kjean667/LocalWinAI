using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Infrastructure.Sessions;

/// <summary>
/// Persists chat sessions under <c>%LocalAppData%\LocalWinAI\sessions\</c>.
/// Each session is stored as <c>{id}.json</c>. A lightweight <c>index.json</c> holds
/// <c>{ Id, Title, LastUsedAt, CreatedAt }</c> so the sidebar can be populated without
/// loading every full session file.
/// </summary>
public sealed class ChatSessionRepository : IChatSessionRepository
{
    internal static readonly string StorageRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LocalWinAI",
        "sessions");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<ChatSessionSummary>> GetAllAsync()
    {
        var indexPath = IndexPath();
        if (!File.Exists(indexPath))
            return [];

        var json = await File.ReadAllTextAsync(indexPath);
        var index = JsonSerializer.Deserialize<List<SessionIndexEntry>>(json, JsonOptions) ?? [];

        return index
            .Select(e => new ChatSessionSummary(e.Id, e.Title, e.CreatedAt, e.LastUsedAt))
            .ToList();
    }

    public async Task<ChatSession?> GetAsync(Guid id)
    {
        var path = SessionPath(id);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        var doc = JsonSerializer.Deserialize<ChatSessionDocument>(json, JsonOptions);
        if (doc is null)
            return null;

        var session = new ChatSession { Id = doc.Id, Title = doc.Title, CreatedAt = doc.CreatedAt, LastUsedAt = doc.LastUsedAt, WorkspaceId = doc.WorkspaceId };
        foreach (var m in doc.Messages)
            session.AddMessage(m);
        return session;
    }

    public async Task SaveAsync(ChatSession session)
    {
        Directory.CreateDirectory(StorageRoot);

        var doc = new ChatSessionDocument
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            LastUsedAt = session.LastUsedAt,
            WorkspaceId = session.WorkspaceId,
            Messages = session.Messages.ToList()
        };
        var sessionJson = JsonSerializer.Serialize(doc, JsonOptions);
        await WriteWithRetryAsync(SessionPath(session.Id), sessionJson);
        await UpdateIndexAsync(session);
    }

    public async Task DeleteAsync(Guid id)
    {
        var path = SessionPath(id);
        if (File.Exists(path))
            File.Delete(path);

        var indexPath = IndexPath();
        if (!File.Exists(indexPath))
            return;

        var json = await File.ReadAllTextAsync(indexPath);
        var index = JsonSerializer.Deserialize<List<SessionIndexEntry>>(json, JsonOptions) ?? [];
        index.RemoveAll(e => e.Id == id);
        await WriteWithRetryAsync(indexPath, JsonSerializer.Serialize(index, JsonOptions));
    }

    private async Task UpdateIndexAsync(ChatSession session)
    {
        var indexPath = IndexPath();
        List<SessionIndexEntry> index = [];

        if (File.Exists(indexPath))
        {
            var existing = await File.ReadAllTextAsync(indexPath);
            index = JsonSerializer.Deserialize<List<SessionIndexEntry>>(existing, JsonOptions) ?? [];
        }

        var i = index.FindIndex(e => e.Id == session.Id);
        var entry = new SessionIndexEntry(session.Id, session.Title, session.CreatedAt, session.LastUsedAt);
        if (i >= 0)
            index[i] = entry;
        else
            index.Add(entry);

        await WriteWithRetryAsync(indexPath, JsonSerializer.Serialize(index, JsonOptions));
    }

    private static async Task WriteWithRetryAsync(string path, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    path, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 4096, useAsync: true);
                await stream.WriteAsync(bytes);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                await Task.Delay(50);
            }
        }
    }

    private static string SessionPath(Guid id) => Path.Combine(StorageRoot, $"{id}.json");
    private static string IndexPath() => Path.Combine(StorageRoot, "index.json");

    private sealed record SessionIndexEntry(Guid Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset LastUsedAt);

    private sealed class ChatSessionDocument
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastUsedAt { get; set; }
        public Guid? WorkspaceId { get; set; }
        public List<ChatMessage> Messages { get; set; } = [];
    }
}
