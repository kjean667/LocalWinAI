using LocalWinAI.Application.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace LocalWinAI;

public sealed partial class WorkspacesPage : Page
{
    public WorkspacesPageViewModel ViewModel { get; }

    public WorkspacesPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<WorkspacesPageViewModel>();
        WireEditorFolderPicker();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // Re-wire the folder picker callback whenever the Editor instance changes.
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspacesPageViewModel.Editor))
            WireEditorFolderPicker();
    }

    private void WireEditorFolderPicker()
    {
        if (ViewModel.Editor is { } editor)
            editor.RequestFolderAsync = PickFolderAsync;
    }

    private async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");

        // Associate the picker with the window handle (required on WinUI 3).
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private void RemoveFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is WorkspaceFolderViewModel folder)
            ViewModel.Editor?.RemoveFolderCommand.Execute(folder);
    }

    private void RenameItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is Guid id)
        {
            // Select the workspace, then focus the name field.
            var item = FindWorkspaceItem(id);
            if (item is not null)
                ViewModel.SelectedWorkspace = item;

            // Focus the name box after the editor has loaded.
            _ = FocusNameBoxAsync();
        }
    }

    private async Task FocusNameBoxAsync()
    {
        // Give the UI a tick to bind and render the editor before setting focus.
        await Task.Delay(150);
        WorkspaceNameBox.Focus(FocusState.Programmatic);
        WorkspaceNameBox.SelectAll();
    }

    private async void DuplicateItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is Guid id)
            await ViewModel.DuplicateWorkspaceCommand.ExecuteAsync(id);
    }

    private async void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is Guid id)
            await ConfirmAndDeleteAsync(id);
    }

    private async Task ConfirmAndDeleteAsync(Guid id)
    {
        var item = FindWorkspaceItem(id);
        var name = item?.Name ?? "this workspace";

        var dialog = new ContentDialog
        {
            Title = "Delete workspace",
            Content = $"\"{name}\" will be permanently deleted.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
            await ViewModel.DeleteWorkspaceCommand.ExecuteAsync(id);
    }

    private WorkspaceListItemViewModel? FindWorkspaceItem(Guid id)
    {
        foreach (var w in ViewModel.Workspaces)
            if (w.Id == id) return w;
        return null;
    }
}
