using FluentAssertions;
using LocalWinAI.Application;
using LocalWinAI.Domain.Sessions;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests;

public class ChatServiceTests
{
    private static (ChatService service, FakeLanguageModelService lm, FakeChatSessionManager sm) CreateService()
    {
        var lm = new FakeLanguageModelService { ResponseText = "Fake AI response" };
        var sm = new FakeChatSessionManager();
        return (new ChatService(lm, new FakeUsageTracker(), sm), lm, sm);
    }

    [Fact]
    public async Task SendMessageAsync_WhenModelReady_ReturnsGeneratedResponse()
    {
        var (service, lm, _) = CreateService();
        lm.ResponseText = "Hello there!";

        var response = await service.SendMessageAsync("Hi");

        response.Should().Be("Hello there!");
    }

    [Fact]
    public async Task SendMessageAsync_WhenModelNotReady_ThrowsInvalidOperationException()
    {
        var (service, lm, _) = CreateService();
        lm.IsReady = false;

        var act = () => service.SendMessageAsync("Hi");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageAsync_WhenModelNotReady_DoesNotAddMessageToSession()
    {
        var (service, lm, sm) = CreateService();
        lm.IsReady = false;

        try { await service.SendMessageAsync("Hi"); } catch { }

        sm.ActiveSession.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SendMessageAsync_BuildsPromptFromSessionHistory()
    {
        var (service, lm, _) = CreateService();
        lm.ResponseText = "Response 1";
        await service.SendMessageAsync("Message 1");

        lm.ResponseText = "Response 2";
        await service.SendMessageAsync("Message 2");

        lm.LastPrompt.Should().Contain("Message 1");
        lm.LastPrompt.Should().Contain("Message 2");
    }

    [Fact]
    public async Task SendMessageAsync_AddsResponseToHistoryForNextPrompt()
    {
        var (service, lm, _) = CreateService();
        lm.ResponseText = "AI answer";
        await service.SendMessageAsync("Question");

        await service.SendMessageAsync("Follow-up");

        lm.LastPrompt.Should().Contain("AI answer");
    }

    [Fact]
    public async Task SendMessageAsync_NewActiveSession_PromptsContainOnlyNewSessionMessages()
    {
        var (service, lm, sm) = CreateService();
        lm.ResponseText = "Response 1";
        await service.SendMessageAsync("Message 1");

        sm.ActiveSession = new ChatSession();
        lm.ResponseText = "Response 2";
        await service.SendMessageAsync("Message 2");

        lm.LastPrompt.Should().Be("Message 2");
        lm.LastPrompt.Should().NotContain("Message 1");
    }

    [Fact]
    public async Task SendMessageAsync_PersistsSessionAfterSuccessfulExchange()
    {
        var (service, _, sm) = CreateService();

        await service.SendMessageAsync("Hello");

        sm.PersistCallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendMessageAsync_AppendsBothMessagesToSession()
    {
        var (service, lm, sm) = CreateService();
        lm.ResponseText = "World";

        await service.SendMessageAsync("Hello");

        sm.ActiveSession.Messages.Should().HaveCount(2);
        sm.ActiveSession.Messages[0].Text.Should().Be("Hello");
        sm.ActiveSession.Messages[1].Text.Should().Be("World");
    }
}
