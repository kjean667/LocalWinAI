using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalWinAI.Domain;

namespace LocalWinAI.Application;

public partial class ChatPageViewModel : ObservableObject
{
    private readonly IChatService _chatService;

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;

    public ObservableCollection<ObservableChatMessage> ChatMessages { get; } = [];

    public AsyncRelayCommand GenerateResponseCommand { get; }
    public RelayCommand ClearConversationCommand { get; }

    public ChatPageViewModel(IChatService chatService)
    {
        _chatService = chatService;
        GenerateResponseCommand = new AsyncRelayCommand(GenerateResponseAsync);
        ClearConversationCommand = new RelayCommand(ClearConversation);
    }

    private void ClearConversation()
    {
        InputText = string.Empty;
        ChatMessages.Clear();
        _chatService.ClearConversation();
    }

    private async Task GenerateResponseAsync()
    {
        var userMessage = InputText.Trim();
        if (string.IsNullOrEmpty(userMessage))
            return;

        IsBusy = true;
        InputText = string.Empty;
        ChatMessages.Add(new ObservableChatMessage { Text = userMessage, Sender = ChatMessageSender.User });

        var aiMessage = new ObservableChatMessage { Text = "...", Sender = ChatMessageSender.AI, IsWaiting = true };
        ChatMessages.Add(aiMessage);

        try
        {
            var response = await _chatService.SendMessageAsync(userMessage);
            aiMessage.Text = response;
            aiMessage.IsWaiting = false;
        }
        catch (Exception e)
        {
            aiMessage.Text = $"Error: {e.Message}";
            aiMessage.IsWaiting = false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
