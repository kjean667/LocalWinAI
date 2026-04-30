using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalWinAI.Application.Workspaces;

/// <summary>Observable list-item model for the workspaces sidebar.</summary>
public partial class WorkspaceListItemViewModel : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IconGlyph { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AccentColorHex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
}
