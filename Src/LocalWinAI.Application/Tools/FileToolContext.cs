using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Tools;

/// <summary>Concrete <see cref="IFileToolContext"/> that enforces sandbox boundaries for a workspace's folder list.</summary>
public sealed class FileToolContext : IFileToolContext
{
    public IReadOnlyList<WorkspaceFolder> Folders { get; }

    public FileToolContext(IReadOnlyList<WorkspaceFolder> folders)
    {
        if (folders.Count == 0)
            throw new ArgumentException("At least one folder is required.", nameof(folders));
        Folders = folders;
    }

    public bool TryResolveAbsolute(string? folderAlias, string relativePath, out string absolute, out string? error)
    {
        absolute = string.Empty;
        error = null;

        WorkspaceFolder? folder;
        if (string.IsNullOrWhiteSpace(folderAlias))
        {
            if (Folders.Count == 1)
            {
                folder = Folders[0];
            }
            else
            {
                error = $"missing 'folder' — choose one of: {ListAliases()}";
                return false;
            }
        }
        else
        {
            // Alias takes precedence over bare folder name
            folder = Folders.FirstOrDefault(f => string.Equals(f.Alias, folderAlias, StringComparison.OrdinalIgnoreCase))
                  ?? Folders.FirstOrDefault(f => string.Equals(FolderName(f), folderAlias, StringComparison.OrdinalIgnoreCase));

            if (folder is null)
            {
                error = $"unknown folder '{folderAlias}' — choose one of: {ListAliases()}";
                return false;
            }
        }

        string root;
        try
        {
            root = Path.GetFullPath(folder.Path);
            absolute = string.IsNullOrWhiteSpace(relativePath) || relativePath == "."
                ? root
                : Path.GetFullPath(Path.Combine(root, relativePath));
        }
        catch (Exception ex)
        {
            error = $"Invalid path: {ex.Message}";
            return false;
        }

        if (!IsInsideFolder(root, absolute))
        {
            error = $"Path '{relativePath}' escapes the folder root and is not allowed.";
            absolute = string.Empty;
            return false;
        }

        return true;
    }

    private string ListAliases() =>
        string.Join(", ", Folders.Select(f => f.Alias ?? FolderName(f)));

    private static string FolderName(WorkspaceFolder f) =>
        Path.GetFileName(f.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    private static bool IsInsideFolder(string resolvedRoot, string fullPath)
    {
        var rootWithSep = resolvedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                          + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase)
               || string.Equals(fullPath, resolvedRoot, StringComparison.OrdinalIgnoreCase);
    }
}
