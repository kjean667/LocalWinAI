using System.Collections.Specialized;
using System.ComponentModel;
using LocalWinAI.Application;
using LocalWinAI.Application.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.UI.Core;

namespace LocalWinAI;

public sealed partial class ChatPage : Page
{
    public ChatPageViewModel ViewModel { get; }
    private ObservableChatMessage? _lastMessage;
    private bool _suppressSelectionChanged;

    public ChatPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ChatPageViewModel>();
        ViewModel.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        SubscribeToLastMessage();
        Loaded += async (_, _) => await ViewModel.InitializeAsync();
    }

    private async void ChatMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ResponseScrollViewer is null) return;
        SubscribeToLastMessage();
        ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
        await Task.Delay(500);
        ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
    }

    private void SubscribeToLastMessage()
    {
        if (_lastMessage is not null)
            _lastMessage.PropertyChanged -= LastMessage_PropertyChanged;

        if (ViewModel.ChatMessages.Count > 0)
        {
            _lastMessage = ViewModel.ChatMessages[^1];
            _lastMessage.PropertyChanged += LastMessage_PropertyChanged;
        }
        else
        {
            _lastMessage = null;
        }
    }

    private async void LastMessage_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObservableChatMessage.Text) && ResponseScrollViewer is not null)
        {
            await Task.Delay(500);
            ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
        }
    }

    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrl = (InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
            & CoreVirtualKeyStates.Down) != 0;

        if (e.Key == Windows.System.VirtualKey.Enter && ctrl)
            return;

        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.GenerateResponseCommand.CanExecute(null))
        {
            ViewModel.GenerateResponseCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SessionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChanged) return;
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ChatSessionSummaryViewModel vm)
            ViewModel.SwitchSessionCommand.Execute(vm.Id);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ChatPageViewModel.ActiveSessionId)) return;

        _suppressSelectionChanged = true;
        try
        {
            SessionListView.SelectedItem = ViewModel.Sessions
                .FirstOrDefault(s => s.Id == ViewModel.ActiveSessionId);
        }
        finally
        {
            _suppressSelectionChanged = false;
        }
    }

    private void DeleteSession_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is Guid id)
            ViewModel.DeleteSessionCommand.Execute(id);
    }
}
