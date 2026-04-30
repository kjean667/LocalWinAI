using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalWinAI.Application.Workspaces;

/// <summary>Observable model for a single workspace folder entry.</summary>
public partial class WorkspaceFolderViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Path { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Alias { get; set; }
}
