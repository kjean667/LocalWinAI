namespace LocalWinAI.Domain.Workspaces;

/// <summary>A file-system folder included in a workspace, with an optional display alias.</summary>
public sealed record WorkspaceFolder(string Path, string? Alias = null);
