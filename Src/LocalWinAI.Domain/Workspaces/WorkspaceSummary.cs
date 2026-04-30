namespace LocalWinAI.Domain.Workspaces;

/// <summary>Lightweight, detail-free snapshot of a workspace used for sidebar lists and index queries.</summary>
public sealed record WorkspaceSummary(Guid Id, string Name, string IconGlyph, string AccentColorHex, DateTimeOffset UpdatedAt);
