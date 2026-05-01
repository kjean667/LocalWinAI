using System.Text.Json;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Infrastructure.Tools;

public sealed class ReadFileTool : IFileTool
{
    private const int CharLimit = 32_000;

    public string Name => "read_file";
    public string Description => "Reads the UTF-8 content of a file. Arguments: folder (required when workspace has multiple folders, omit for single-folder workspaces), path (relative file path).";

    public async Task<string> ExecuteAsync(IFileToolContext context, string argumentsJson, CancellationToken cancellationToken)
    {
        var (folder, relativePath) = ParseArgs(argumentsJson);

        if (!context.TryResolveAbsolute(folder, relativePath, out var fullPath, out var error))
            return error!;

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

    private static (string? folder, string path) ParseArgs(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
            return (null, string.Empty);
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var folder = doc.RootElement.TryGetProperty("folder", out var f) ? f.GetString() : null;
            var path = doc.RootElement.TryGetProperty("path", out var p) ? p.GetString() ?? string.Empty : string.Empty;
            return (folder, path);
        }
        catch (JsonException) { }
        return (null, string.Empty);
    }
}
