using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Application.Workspaces;

/// <summary>
/// Page-level ViewModel for the Workspaces master/detail surface.
/// Owns the workspace list (left pane) and creates a <see cref="WorkspaceEditorViewModel"/>
/// for the selected workspace (right pane).
/// </summary>
public partial class WorkspacesPageViewModel : ObservableObject, IDisposable
{
    private readonly IWorkspaceManager _manager;
    private readonly SynchronizationContext? _syncContext;

    // Prevents LoadEditorAsync from firing during list rebuilds (e.g. after save/delete).
    private bool _suppressEditorLoad;

    [ObservableProperty]
    public partial WorkspaceListItemViewModel? SelectedWorkspace { get; set; }

    [ObservableProperty]
    public partial WorkspaceEditorViewModel? Editor { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public ObservableCollection<WorkspaceListItemViewModel> Workspaces { get; } = [];

    public bool HasEditor => Editor is not null;

    public IAsyncRelayCommand NewWorkspaceCommand { get; }
    public IAsyncRelayCommand<Guid> DeleteWorkspaceCommand { get; }
    public IAsyncRelayCommand<Guid> DuplicateWorkspaceCommand { get; }
    public IAsyncRelayCommand DiscardCommand { get; }

    public WorkspacesPageViewModel(IWorkspaceManager manager)
    {
        _manager = manager;
        _syncContext = SynchronizationContext.Current;

        _manager.WorkspacesChanged += OnWorkspacesChanged;
        RebuildWorkspaces();

        NewWorkspaceCommand = new AsyncRelayCommand(NewWorkspaceAsync);
        DeleteWorkspaceCommand = new AsyncRelayCommand<Guid>(DeleteWorkspaceAsync);
        DuplicateWorkspaceCommand = new AsyncRelayCommand<Guid>(DuplicateWorkspaceAsync);
        DiscardCommand = new AsyncRelayCommand(DiscardAsync);
    }

    public void Dispose()
    {
        _manager.WorkspacesChanged -= OnWorkspacesChanged;
    }

    partial void OnSelectedWorkspaceChanged(WorkspaceListItemViewModel? value)
    {
        if (!_suppressEditorLoad)
            _ = LoadEditorAsync(value);
    }

    partial void OnEditorChanged(WorkspaceEditorViewModel? value)
    {
        OnPropertyChanged(nameof(HasEditor));
    }

    private void OnWorkspacesChanged(object? sender, EventArgs e)
    {
        if (_syncContext is not null)
            _syncContext.Post(_ => RebuildWorkspaces(), null);
        else
            RebuildWorkspaces();
    }

    private void RebuildWorkspaces()
    {
        var selectedId = SelectedWorkspace?.Id;

        _suppressEditorLoad = true;
        try
        {
            Workspaces.Clear();
            foreach (var s in _manager.Summaries)
            {
                Workspaces.Add(new WorkspaceListItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IconGlyph = s.IconGlyph,
                    AccentColorHex = s.AccentColorHex,
                });
            }

            IsEmpty = Workspaces.Count == 0;

            if (selectedId is not null)
            {
                var newItem = Workspaces.FirstOrDefault(w => w.Id == selectedId);
                if (newItem is null)
                {
                    SelectedWorkspace = null;
                    Editor = null;
                }
                else
                {
                    // Restore selection to new list instance; editor is kept as-is.
                    SelectedWorkspace = newItem;
                }
            }
        }
        finally
        {
            _suppressEditorLoad = false;
        }
    }

    private async Task LoadEditorAsync(WorkspaceListItemViewModel? item)
    {
        if (item is null)
        {
            Editor = null;
            return;
        }

        var workspace = await _manager.GetAsync(item.Id);
        if (workspace is null)
        {
            Editor = null;
            return;
        }

        Editor = new WorkspaceEditorViewModel(workspace, _manager);
    }

    private async Task NewWorkspaceAsync(CancellationToken ct)
    {
        var workspace = await _manager.CreateAsync("Untitled workspace", ct);
        var item = Workspaces.FirstOrDefault(w => w.Id == workspace.Id);
        SelectedWorkspace = item;
    }

    private Task DeleteWorkspaceAsync(Guid id, CancellationToken ct)
        => _manager.DeleteAsync(id, ct);

    private Task DiscardAsync(CancellationToken ct)
        => LoadEditorAsync(SelectedWorkspace);

    private async Task DuplicateWorkspaceAsync(Guid id, CancellationToken ct)
    {
        var source = await _manager.GetAsync(id, ct);
        if (source is null)
            return;

        var copy = await _manager.CreateAsync(source.Name + " (Copy)", ct);
        copy.Description = source.Description;
        copy.IconGlyph = source.IconGlyph;
        copy.AccentColorHex = source.AccentColorHex;
        copy.SystemPrompt = source.SystemPrompt;
        foreach (var f in source.Folders)
            copy.Folders.Add(new WorkspaceFolder(f.Path, f.Alias));
        await _manager.SaveAsync(copy, ct);

        var item = Workspaces.FirstOrDefault(w => w.Id == copy.Id);
        SelectedWorkspace = item;
    }
}
