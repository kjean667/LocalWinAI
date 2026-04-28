using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalWinAI.Application.Sessions;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Sessions;

namespace LocalWinAI.Application;

public partial class ChatPageViewModel : ObservableObject
{
    private readonly IChatService _chatService;
    private readonly IChatSessionManager _sessionManager;
    private readonly SynchronizationContext? _syncContext;

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;

    [ObservableProperty]
    public partial Guid ActiveSessionId { get; set; }

    public ObservableCollection<ObservableChatMessage> ChatMessages { get; } = [];
    public ObservableCollection<ChatSessionSummaryViewModel> Sessions { get; } = [];

    public AsyncRelayCommand GenerateResponseCommand { get; }
    public AsyncRelayCommand NewSessionCommand { get; }
    public AsyncRelayCommand<Guid> SwitchSessionCommand { get; }
    public AsyncRelayCommand<Guid> DeleteSessionCommand { get; }

    public ChatPageViewModel(IChatService chatService, IChatSessionManager sessionManager)
    {
        _chatService = chatService;
        _sessionManager = sessionManager;
        _syncContext = SynchronizationContext.Current;

        GenerateResponseCommand = new AsyncRelayCommand(GenerateResponseAsync);
        NewSessionCommand = new AsyncRelayCommand(NewSessionAsync);
        SwitchSessionCommand = new AsyncRelayCommand<Guid>(SwitchSessionAsync);
        DeleteSessionCommand = new AsyncRelayCommand<Guid>(DeleteSessionAsync);

        _sessionManager.SessionTitleUpdated += OnSessionTitleUpdated;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _sessionManager.InitializeAsync(cancellationToken);
        await RefreshSessionsAsync();
        LoadActiveSessionMessages();
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

    private async Task NewSessionAsync()
    {
        await _sessionManager.CreateSessionAsync();
        await RefreshSessionsAsync();
        LoadActiveSessionMessages();
    }

    private async Task SwitchSessionAsync(Guid id)
    {
        if (id == _sessionManager.ActiveSession.Id)
            return;

        await _sessionManager.SwitchToSessionAsync(id);
        LoadActiveSessionMessages();
        ActiveSessionId = _sessionManager.ActiveSession.Id;
    }

    private async Task DeleteSessionAsync(Guid id)
    {
        await _sessionManager.DeleteSessionAsync(id);
        await RefreshSessionsAsync();
        LoadActiveSessionMessages();
    }

    private async Task RefreshSessionsAsync()
    {
        var sessions = await _sessionManager.GetSessionsAsync();
        Sessions.Clear();
        foreach (var s in sessions.OrderByDescending(s => s.LastUsedAt))
            Sessions.Add(ToSummary(s));
        ActiveSessionId = _sessionManager.ActiveSession.Id;
    }

    private void LoadActiveSessionMessages()
    {
        ChatMessages.Clear();
        foreach (var msg in _sessionManager.ActiveSession.Messages)
        {
            ChatMessages.Add(new ObservableChatMessage
            {
                Sender = msg.Sender,
                Text = msg.Text
            });
        }
        ActiveSessionId = _sessionManager.ActiveSession.Id;
    }

    private void OnSessionTitleUpdated(object? sender, ChatSession session)
    {
        if (_syncContext is not null)
            _syncContext.Post(_ => ApplyTitleUpdate(session), null);
        else
            ApplyTitleUpdate(session);
    }

    private void ApplyTitleUpdate(ChatSession session)
    {
        var vm = Sessions.FirstOrDefault(s => s.Id == session.Id);
        if (vm is not null)
            vm.Title = session.Title;
    }

    private static ChatSessionSummaryViewModel ToSummary(ChatSession s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        LastUsedAt = s.LastUsedAt
    };
}
