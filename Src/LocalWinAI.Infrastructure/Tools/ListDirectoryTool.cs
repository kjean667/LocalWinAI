using System.Text;
using System.Text.Json;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Infrastructure.Tools;

public sealed class ListDirectoryTool : IFileTool
{
    public string Name => "list_directory";
    public string Description => "Lists files and subdirectories at a path relative to a workspace folder. Arguments: folder (required when workspace has multiple folders, omit for single-folder workspaces), path (relative directory path).";

    public Task<string> ExecuteAsync(IFileToolContext context, string argumentsJson, CancellationToken cancellationToken)
    {
        var (folder, relativePath) = ParseArgs(argumentsJson);

        if (!context.TryResolveAbsolute(folder, relativePath, out var fullPath, out var error))
            return Task.FromResult(error!);

        if (!Directory.Exists(fullPath))
            return Task.FromResult($"Directory not found: {relativePath}");

        var sb = new StringBuilder();
        foreach (var dir in Directory.GetDirectories(fullPath).OrderBy(d => d))
            sb.AppendLine($"[dir]  {Path.GetFileName(dir)}");
        foreach (var file in Directory.GetFiles(fullPath).OrderBy(f => f))
            sb.AppendLine($"[file] {Path.GetFileName(file)}");

        return Task.FromResult(sb.Length == 0 ? "(empty directory)" : sb.ToString().TrimEnd());
    }

    private static (string? folder, string path) ParseArgs(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
            return (null, ".");
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var folder = doc.RootElement.TryGetProperty("folder", out var f) ? f.GetString() : null;
            var path = doc.RootElement.TryGetProperty("path", out var p) ? p.GetString() ?? "." : ".";
            return (folder, path);
        }
        catch (JsonException) { }
        return (null, ".");
    }
}
