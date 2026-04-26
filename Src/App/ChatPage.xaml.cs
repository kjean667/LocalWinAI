using LocalWinAI.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LocalWinAI;

public sealed partial class ChatPage : Page
{
    public ChatPageViewModel ViewModel { get; }
    private ObservableChatMessage? _lastMessage;

    public ChatPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ChatPageViewModel>();
        ViewModel.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
        SubscribeToLastMessage();
    }

    private async void ChatMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ResponseScrollViewer != null)
        {
            SubscribeToLastMessage();
            ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
            await Task.Delay(500);
            ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
        }
    }

    private void SubscribeToLastMessage()
    {
        if (_lastMessage != null)
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
        if (e.PropertyName == nameof(ObservableChatMessage.Text) && ResponseScrollViewer != null)
        {
            await Task.Delay(500);
            ResponseScrollViewer.ChangeView(null, double.MaxValue, null);
        }
    }

    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.GenerateResponseCommand.CanExecute(null))
        {
            ViewModel.GenerateResponseCommand.Execute(null);
            e.Handled = true;
        }
    }
}
