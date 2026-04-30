using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Workspaces;

/// <summary>
/// Editor ViewModel for a single workspace's basic fields.
/// Constructed by <see cref="WorkspacesPageViewModel"/> when the user selects a workspace — not registered in DI.
/// </summary>
public partial class WorkspaceEditorViewModel : ObservableObject
{
    private readonly Workspace _workspace;
    private readonly IWorkspaceManager _manager;

    /// <summary>
    /// Set by the page (which owns XamlRoot) before any folder commands run.
    /// Returns the chosen folder path, or null if the user cancels.
    /// </summary>
    public Func<Task<string?>>? RequestFolderAsync { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IconGlyph { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AccentColorHex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SystemPrompt { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDirty { get; set; }

    public ObservableCollection<WorkspaceFolderViewModel> Folders { get; } = [];

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand AddFolderCommand { get; }
    public IRelayCommand<WorkspaceFolderViewModel> RemoveFolderCommand { get; }

    public WorkspaceEditorViewModel(Workspace workspace, IWorkspaceManager manager)
    {
        _workspace = workspace;
        _manager = manager;

        // Initialize via property setters so PropertyChanged fires for bindings,
        // but _initialized keeps IsDirty from flipping during construction.
        Name = workspace.Name;
        Description = workspace.Description;
        IconGlyph = workspace.IconGlyph;
        AccentColorHex = workspace.AccentColorHex;
        SystemPrompt = workspace.SystemPrompt;

        foreach (var f in workspace.Folders)
            Folders.Add(new WorkspaceFolderViewModel { Path = f.Path, Alias = f.Alias });

        // Mark initialized after all fields are set so dirty tracking begins.
        _initialized = true;
        IsDirty = false;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync);
        RemoveFolderCommand = new RelayCommand<WorkspaceFolderViewModel>(RemoveFolder);
    }

    // Guards IsDirty from flipping during constructor initialization.
    private bool _initialized;

    partial void OnNameChanged(string value) { if (_initialized) IsDirty = true; }
    partial void OnDescriptionChanged(string value) { if (_initialized) IsDirty = true; }
    partial void OnIconGlyphChanged(string value) { if (_initialized) IsDirty = true; }
    partial void OnAccentColorHexChanged(string value) { if (_initialized) IsDirty = true; }
    partial void OnSystemPromptChanged(string value) { if (_initialized) IsDirty = true; }

    private async Task SaveAsync()
    {
        _workspace.Name = Name;
        _workspace.Description = Description;
        _workspace.IconGlyph = IconGlyph;
        _workspace.AccentColorHex = AccentColorHex;
        _workspace.SystemPrompt = SystemPrompt;

        _workspace.Folders.Clear();
        foreach (var fvm in Folders)
            _workspace.Folders.Add(new WorkspaceFolder(fvm.Path, fvm.Alias));

        await _manager.SaveAsync(_workspace);
        IsDirty = false;
    }

    private async Task AddFolderAsync()
    {
        if (RequestFolderAsync is null)
            return;

        var path = await RequestFolderAsync();
        if (string.IsNullOrEmpty(path))
            return;

        Folders.Add(new WorkspaceFolderViewModel { Path = path });
        IsDirty = true;
    }

    private void RemoveFolder(WorkspaceFolderViewModel? folder)
    {
        if (folder is null)
            return;

        Folders.Remove(folder);
        IsDirty = true;
    }
}
