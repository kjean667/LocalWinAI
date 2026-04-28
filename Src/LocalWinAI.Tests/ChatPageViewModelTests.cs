using FluentAssertions;
using LocalWinAI.Application;
using LocalWinAI.Domain;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests;

public class ChatPageViewModelTests
{
    private static (ChatPageViewModel vm, FakeChatService chatService, FakeChatSessionManager sessionManager) CreateVm()
    {
        // Clear the xUnit SynchronizationContext so the ViewModel dispatches title updates
        // synchronously during tests (the real app captures the WinUI dispatcher context).
        var prev = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            var chatService = new FakeChatService();
            var sessionManager = new FakeChatSessionManager();
            return (new ChatPageViewModel(chatService, sessionManager), chatService, sessionManager);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prev);
        }
    }

    [Fact]
    public async Task GenerateResponseCommand_AddsUserMessageThenAiResponse()
    {
        var (vm, chatService, _) = CreateVm();
        chatService.Response = "AI says hi";
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().HaveCount(2);
        vm.ChatMessages[0].Sender.Should().Be(ChatMessageSender.User);
        vm.ChatMessages[0].Text.Should().Be("Hello");
        vm.ChatMessages[1].Sender.Should().Be(ChatMessageSender.AI);
        vm.ChatMessages[1].Text.Should().Be("AI says hi");
        vm.ChatMessages[1].IsWaiting.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateResponseCommand_ClearsInputText()
    {
        var (vm, _, _) = CreateVm();
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.InputText.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateResponseCommand_WhenModelFails_ShowsErrorInAiMessage()
    {
        var (vm, chatService, _) = CreateVm();
        chatService.ThrowOnSend = true;
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().HaveCount(2);
        vm.ChatMessages[1].Text.Should().StartWith("Error:");
        vm.ChatMessages[1].IsWaiting.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateResponseCommand_WithEmptyInput_DoesNothing()
    {
        var (vm, chatService, _) = CreateVm();
        vm.InputText = "   ";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().BeEmpty();
        chatService.SendCallCount.Should().Be(0);
    }

    [Fact]
    public async Task GenerateResponseCommand_IsBusyResetAfterCompletion()
    {
        var (vm, _, _) = CreateVm();
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_LoadsSessionsAndMessages()
    {
        var (vm, _, sessionManager) = CreateVm();
        sessionManager.ActiveSession.AddMessage(
            new LocalWinAI.Domain.Sessions.ChatMessage(ChatMessageSender.User, "Hi", DateTimeOffset.UtcNow));

        await vm.InitializeAsync();

        vm.Sessions.Should().HaveCount(1);
        vm.ChatMessages.Should().HaveCount(1);
        vm.ChatMessages[0].Text.Should().Be("Hi");
    }

    [Fact]
    public async Task NewSessionCommand_ClearsMessagesAndAddsSession()
    {
        var (vm, _, sessionManager) = CreateVm();
        await vm.InitializeAsync();
        vm.ChatMessages.Add(new ObservableChatMessage { Sender = ChatMessageSender.User, Text = "Old message" });

        await vm.NewSessionCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().BeEmpty();
        sessionManager.AllSessions.Should().HaveCount(2);
    }

    [Fact]
    public async Task SwitchSessionCommand_LoadsTargetSessionMessages()
    {
        var (vm, _, sessionManager) = CreateVm();
        await vm.InitializeAsync();

        // Add a second session directly so active session stays as session 1.
        var other = new LocalWinAI.Domain.Sessions.ChatSession();
        other.AddMessage(new LocalWinAI.Domain.Sessions.ChatMessage(ChatMessageSender.User, "Other session", DateTimeOffset.UtcNow));
        sessionManager.AllSessions.Add(other);

        await vm.SwitchSessionCommand.ExecuteAsync(other.Id);

        vm.ChatMessages.Should().HaveCount(1);
        vm.ChatMessages[0].Text.Should().Be("Other session");
    }

    [Fact]
    public async Task DeleteSessionCommand_RemovesSessionFromSidebar()
    {
        var (vm, _, sessionManager) = CreateVm();
        await vm.InitializeAsync();
        var toDelete = sessionManager.ActiveSession;

        await vm.DeleteSessionCommand.ExecuteAsync(toDelete.Id);

        vm.Sessions.Should().NotContain(s => s.Id == toDelete.Id);
    }

    [Fact]
    public void SessionTitleUpdated_Event_UpdatesTitleInSessions()
    {
        var (vm, _, sessionManager) = CreateVm();
        var session = sessionManager.ActiveSession;
        vm.Sessions.Add(new LocalWinAI.Application.Sessions.ChatSessionSummaryViewModel
        {
            Id = session.Id,
            Title = "Old title",
            LastUsedAt = session.LastUsedAt
        });
        session.Title = "New title";

        sessionManager.RaiseSessionTitleUpdated(session);

        vm.Sessions[0].Title.Should().Be("New title");
    }
}
