namespace LocalWinAI.Domain.Workspaces;

/// <summary>A pinned context file included in a workspace, with an optional description.</summary>
public sealed record WorkspaceContextFile(string Path, string? Description = null);
