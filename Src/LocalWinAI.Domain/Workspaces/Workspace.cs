namespace LocalWinAI.Domain.Workspaces;

/// <summary>Aggregate root for a named, user-configured workspace bundle.</summary>
public sealed class Workspace
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "New workspace";
    public string Description { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "";   // Segoe MDL2 "Folder"
    public string AccentColorHex { get; set; } = "#0078D4";
    public string SystemPrompt { get; set; } = string.Empty;
    public bool MemoryEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public IList<WorkspaceFolder> Folders { get; init; } = new List<WorkspaceFolder>();
    public IList<WorkspaceContextFile> ContextFiles { get; init; } = new List<WorkspaceContextFile>();
    public IList<string> StarterPrompts { get; init; } = new List<string>();

    /// <summary>null means all tools are allowed.</summary>
    public IList<string>? AllowedTools { get; set; }
}
