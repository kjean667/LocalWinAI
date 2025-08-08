using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalWinAI;

public enum ChatMessageSender
{
    User,
    AI
}

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;
    [ObservableProperty]
    private ChatMessageSender _sender = ChatMessageSender.User;
    [ObservableProperty]
    private bool _isWaiting = false;
}
