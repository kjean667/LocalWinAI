using System.Text;
using System.Text.Json;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Infrastructure.Tools;

public sealed class ListDirectoryTool : IFileTool
{
    public string Name => "list_directory";
    public string Description => "Lists files and subdirectories at a path relative to the workspace root.";

    public Task<string> ExecuteAsync(string workspaceRoot, string argumentsJson, CancellationToken cancellationToken)
    {
        var relativePath = ParsePath(argumentsJson);

        if (!FileToolSandbox.TryResolve(workspaceRoot, relativePath, out var fullPath, out var error))
            return Task.FromResult(error);

        if (!Directory.Exists(fullPath))
            return Task.FromResult($"Directory not found: {relativePath}");

        var sb = new StringBuilder();
        foreach (var dir in Directory.GetDirectories(fullPath).OrderBy(d => d))
            sb.AppendLine($"[dir]  {Path.GetFileName(dir)}");
        foreach (var file in Directory.GetFiles(fullPath).OrderBy(f => f))
            sb.AppendLine($"[file] {Path.GetFileName(file)}");

        return Task.FromResult(sb.Length == 0 ? "(empty directory)" : sb.ToString().TrimEnd());
    }

    private static string ParsePath(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
            return ".";
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("path", out var p))
                return p.GetString() ?? ".";
        }
        catch (JsonException) { }
        return ".";
    }
}
