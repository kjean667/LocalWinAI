namespace LocalWinAI.Infrastructure.Tools;

internal static class FileToolSandbox
{
    internal static bool TryResolve(string workspaceRoot, string relativePath, out string fullPath, out string error)
    {
        fullPath = string.Empty;
        error = string.Empty;
        string root;

        try
        {
            root = Path.GetFullPath(workspaceRoot);
            fullPath = string.IsNullOrWhiteSpace(relativePath) || relativePath == "."
                ? root
                : Path.GetFullPath(Path.Combine(root, relativePath));
        }
        catch (Exception ex)
        {
            error = $"Invalid path: {ex.Message}";
            return false;
        }

        if (!IsInsideWorkspace(root, fullPath))
        {
            error = $"Path '{relativePath}' escapes the workspace root and is not allowed.";
            fullPath = string.Empty;
            return false;
        }

        return true;
    }

    private static bool IsInsideWorkspace(string resolvedRoot, string fullPath)
    {
        var rootWithSep = resolvedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                          + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase)
               || string.Equals(fullPath, resolvedRoot, StringComparison.OrdinalIgnoreCase);
    }
}
