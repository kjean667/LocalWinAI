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
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ChatMessageSender Sender { get; set; } = ChatMessageSender.User;

    [ObservableProperty]
    public partial bool IsWaiting { get; set; } = false;
}
