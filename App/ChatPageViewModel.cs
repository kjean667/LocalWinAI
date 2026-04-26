using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LocalWinAI;

public partial class ChatPageViewModel : ObservableObject
{
    public AsyncRelayCommand GenerateResponseCommand { get; }
    public RelayCommand ClearConversationCommand { get; }

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;

    private LanguageModel? _languageModel = null;

    [ObservableProperty]
    public partial ObservableCollection<ChatMessage> ChatMessages { get; set; } = new();

    public ChatPageViewModel()
    {
        ClearConversationCommand = new RelayCommand(ClearConversation);
        GenerateResponseCommand = new AsyncRelayCommand(GenerateResponseAsync);
    }

    private void ClearConversation()
    {
        InputText = string.Empty;
        ChatMessages.Clear();
    }

    private async Task GenerateResponseAsync()
    {
        IsBusy = true;
        var userMessage = new ChatMessage { Text = InputText, Sender = ChatMessageSender.User };
        ChatMessages.Add(userMessage);
        InputText = string.Empty;
        var aiWaitingMessage = new ChatMessage { Text = "...", Sender = ChatMessageSender.AI, IsWaiting = true };
        ChatMessages.Add(aiWaitingMessage);
        try
        {
            if (LanguageModel.GetReadyState() != AIFeatureReadyState.Ready)
            {
                var op = await LanguageModel.EnsureReadyAsync();
                if (op.Status != AIFeatureReadyResultState.Success)
                {
                    aiWaitingMessage.Text = "Language model is not ready: " + op.Status;
                    aiWaitingMessage.IsWaiting = false;
                    return;
                }
            }

            if (_languageModel == null)
            {
                _languageModel = await LanguageModel.CreateAsync();
            }
            var prompt = string.Join("\n", ChatMessages.Select(m => m.Text));
            var result = await _languageModel.GenerateResponseAsync(prompt);
            aiWaitingMessage.Text = result.Text;
            aiWaitingMessage.IsWaiting = false;
        }
        catch(Exception e)
        {
            aiWaitingMessage.Text = $"Exception: {e.Message}";
            aiWaitingMessage.IsWaiting = false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
