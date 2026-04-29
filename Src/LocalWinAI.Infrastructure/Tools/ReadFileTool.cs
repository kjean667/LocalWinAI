using System.Text.Json;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Infrastructure.Tools;

public sealed class ReadFileTool : IFileTool
{
    private const int CharLimit = 32_000;

    public string Name => "read_file";
    public string Description => "Reads the UTF-8 content of a file at a path relative to the workspace root.";

    public async Task<string> ExecuteAsync(string workspaceRoot, string argumentsJson, CancellationToken cancellationToken)
    {
        var relativePath = ParsePath(argumentsJson);

        if (!FileToolSandbox.TryResolve(workspaceRoot, relativePath, out var fullPath, out var error))
            return error;

        if (!File.Exists(fullPath))
            return $"File not found: {relativePath}";

        var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
        if (content.Length > CharLimit)
        {
            // Step back one char if we're about to split a surrogate pair
            var cutAt = char.IsHighSurrogate(content[CharLimit - 1]) ? CharLimit - 1 : CharLimit;
            return string.Concat(content.AsSpan(0, cutAt), "\n[truncated]");
        }

        return content;
    }

    private static string ParsePath(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
            return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("path", out var p))
                return p.GetString() ?? string.Empty;
        }
        catch (JsonException) { }
        return string.Empty;
    }
}
