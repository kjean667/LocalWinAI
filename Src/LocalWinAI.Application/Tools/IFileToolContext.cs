using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Tools;

/// <summary>Provides folder resolution and sandbox enforcement for file tool execution within a workspace.</summary>
public interface IFileToolContext
{
    IReadOnlyList<WorkspaceFolder> Folders { get; }

    /// <summary>
    /// Resolves <paramref name="folderAlias"/> and <paramref name="relativePath"/> to an absolute path.
    /// When the workspace has a single folder, <paramref name="folderAlias"/> may be null or empty.
    /// Returns false and sets <paramref name="error"/> when resolution fails.
    /// </summary>
    bool TryResolveAbsolute(string? folderAlias, string relativePath, out string absolute, out string? error);
}
