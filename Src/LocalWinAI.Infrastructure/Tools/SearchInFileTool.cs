using System.Text;
using System.Text.Json;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Infrastructure.Tools;

public sealed class SearchInFileTool : IFileTool
{
    public string Name => "search_in_file";
    public string Description => "Searches for a query string (case-insensitive) in a file relative to the workspace root. Returns matching lines formatted as 'line_number: content'.";

    public async Task<string> ExecuteAsync(string workspaceRoot, string argumentsJson, CancellationToken cancellationToken)
    {
        var (relativePath, query) = ParseArgs(argumentsJson);

        if (string.IsNullOrWhiteSpace(query))
            return "Missing required argument: query";

        if (string.IsNullOrWhiteSpace(relativePath))
            return "Missing required argument: path";

        if (!FileToolSandbox.TryResolve(workspaceRoot, relativePath, out var fullPath, out var error))
            return error;

        if (!File.Exists(fullPath))
            return $"File not found: {relativePath}";

        var lines = await File.ReadAllLinesAsync(fullPath, cancellationToken);
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                sb.AppendLine($"{i + 1}: {lines[i]}");
        }

        return sb.Length == 0 ? $"No matches found for '{query}'." : sb.ToString().TrimEnd();
    }

    private static (string path, string query) ParseArgs(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
            return (string.Empty, string.Empty);
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path = doc.RootElement.TryGetProperty("path", out var p) ? p.GetString() ?? string.Empty : string.Empty;
            var query = doc.RootElement.TryGetProperty("query", out var q) ? q.GetString() ?? string.Empty : string.Empty;
            return (path, query);
        }
        catch (JsonException)
        {
            return (string.Empty, string.Empty);
        }
    }
}
