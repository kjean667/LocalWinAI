using CommunityToolkit.Mvvm.ComponentModel;
using LocalWinAI.Domain;

namespace LocalWinAI.Application;

public partial class ObservableChatMessage : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsWaiting { get; set; } = false;

    public required ChatMessageSender Sender { get; init; }
}
