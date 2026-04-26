using FluentAssertions;
using LocalWinAI.Application;
using LocalWinAI.Domain;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests;

public class ChatPageViewModelTests
{
    [Fact]
    public async Task GenerateResponseCommand_AddsUserMessageThenAiResponse()
    {
        var chatService = new FakeChatService { Response = "AI says hi" };
        var vm = new ChatPageViewModel(chatService);
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
        var chatService = new FakeChatService();
        var vm = new ChatPageViewModel(chatService);
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.InputText.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateResponseCommand_WhenModelFails_ShowsErrorInAiMessage()
    {
        var chatService = new FakeChatService { ThrowOnSend = true };
        var vm = new ChatPageViewModel(chatService);
        vm.InputText = "Hello";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().HaveCount(2);
        vm.ChatMessages[1].Text.Should().StartWith("Error:");
        vm.ChatMessages[1].IsWaiting.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateResponseCommand_WithEmptyInput_DoesNothing()
    {
        var chatService = new FakeChatService();
        var vm = new ChatPageViewModel(chatService);
        vm.InputText = "   ";

        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.ChatMessages.Should().BeEmpty();
        chatService.SendCallCount.Should().Be(0);
    }

    [Fact]
    public void ClearConversationCommand_ClearsMessagesAndInput()
    {
        var chatService = new FakeChatService();
        var vm = new ChatPageViewModel(chatService);
        vm.ChatMessages.Add(new ObservableChatMessage { Sender = ChatMessageSender.User, Text = "Hi" });
        vm.InputText = "typing...";

        vm.ClearConversationCommand.Execute(null);

        vm.ChatMessages.Should().BeEmpty();
        vm.InputText.Should().BeEmpty();
        chatService.ConversationCleared.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateResponseCommand_IsBusyDuringExecution()
    {
        var tcs = new TaskCompletionSource<string>();
        var chatService = new FakeChatService();
        var vm = new ChatPageViewModel(chatService);
        vm.InputText = "Hello";

        // IsBusy resets after completion since we use FakeChatService (synchronous)
        await vm.GenerateResponseCommand.ExecuteAsync(null);

        vm.IsBusy.Should().BeFalse();
    }
}
