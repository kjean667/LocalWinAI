using FluentAssertions;
using LocalWinAI.Application;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests;

public class ChatServiceTests
{
    [Fact]
    public async Task SendMessageAsync_WhenModelReady_ReturnsGeneratedResponse()
    {
        var fake = new FakeLanguageModelService { ResponseText = "Hello there!" };
        var service = new ChatService(fake);

        var response = await service.SendMessageAsync("Hi");

        response.Should().Be("Hello there!");
    }

    [Fact]
    public async Task SendMessageAsync_WhenModelNotReady_ThrowsInvalidOperationException()
    {
        var fake = new FakeLanguageModelService { IsReady = false };
        var service = new ChatService(fake);

        var act = () => service.SendMessageAsync("Hi");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageAsync_BuildsPromptFromConversationHistory()
    {
        var fake = new FakeLanguageModelService { ResponseText = "Response 2" };
        var service = new ChatService(fake);

        await service.SendMessageAsync("Message 1");
        fake.ResponseText = "Response 2";
        await service.SendMessageAsync("Message 2");

        fake.LastPrompt.Should().Contain("Message 1");
        fake.LastPrompt.Should().Contain("Message 2");
    }

    [Fact]
    public async Task ClearConversation_ResetsHistorySoNextPromptOnlyContainsNewMessage()
    {
        var fake = new FakeLanguageModelService();
        var service = new ChatService(fake);

        await service.SendMessageAsync("Message 1");
        service.ClearConversation();
        await service.SendMessageAsync("Message 2");

        fake.LastPrompt.Should().Be("Message 2");
        fake.LastPrompt.Should().NotContain("Message 1");
    }

    [Fact]
    public async Task SendMessageAsync_AddsResponseToHistoryForNextPrompt()
    {
        var fake = new FakeLanguageModelService { ResponseText = "AI answer" };
        var service = new ChatService(fake);

        await service.SendMessageAsync("Question");
        await service.SendMessageAsync("Follow-up");

        fake.LastPrompt.Should().Contain("AI answer");
    }
}
