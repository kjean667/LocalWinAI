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

    private static (ChatService service, FakeLanguageModelService lm, FakeChatSessionManager sm, FakeToolRegistry registry) CreateServiceWithTools()
    {
        var lm = new FakeLanguageModelService { ResponseText = "Fake AI response" };
        var sm = new FakeChatSessionManager();
        var registry = new FakeToolRegistry();
        return (new ChatService(lm, new FakeUsageTracker(), sm, registry), lm, sm, registry);
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

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_ExecutesToolAndReturnsFollowupResponse()
    {
        var (service, lm, sm, registry) = CreateServiceWithTools();
        sm.ActiveSession.WorkspacePath = @"C:\workspace";
        registry.Register(new FakeFileTool("read_file") { Result = "file contents" });
        lm.ResponseQueue.Enqueue("<tool_call name=\"read_file\">{}</tool_call>");
        lm.ResponseQueue.Enqueue("Here is the answer.");

        var response = await service.SendMessageAsync("Read the file");

        response.Should().Be("Here is the answer.");
    }

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_OnlyUserAndFinalAiMessagePersistedToSession()
    {
        var (service, lm, sm, registry) = CreateServiceWithTools();
        sm.ActiveSession.WorkspacePath = @"C:\workspace";
        registry.Register(new FakeFileTool("read_file") { Result = "file contents" });
        lm.ResponseQueue.Enqueue("<tool_call name=\"read_file\">{}</tool_call>");
        lm.ResponseQueue.Enqueue("Here is the answer.");

        await service.SendMessageAsync("Read the file");

        sm.ActiveSession.Messages.Should().HaveCount(2);
        sm.ActiveSession.Messages[0].Text.Should().Be("Read the file");
        sm.ActiveSession.Messages[1].Text.Should().Be("Here is the answer.");
    }

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_WorkspacePathNull_ReturnsResponseAsIs()
    {
        var (service, lm, _, _) = CreateServiceWithTools();
        // WorkspacePath is null by default
        lm.ResponseText = "<tool_call name=\"read_file\">{}</tool_call>";

        var response = await service.SendMessageAsync("Read the file");

        response.Should().Be("<tool_call name=\"read_file\">{}</tool_call>");
        lm.GenerateCallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_ToolResultAppendedToSubsequentPrompt()
    {
        var (service, lm, sm, registry) = CreateServiceWithTools();
        sm.ActiveSession.WorkspacePath = @"C:\workspace";
        registry.Register(new FakeFileTool("read_file") { Result = "file contents" });
        lm.ResponseQueue.Enqueue("<tool_call name=\"read_file\">{}</tool_call>");
        lm.ResponseQueue.Enqueue("Final answer");

        await service.SendMessageAsync("Hello");

        lm.LastPrompt.Should().Contain("<tool_result name=\"read_file\">");
        lm.LastPrompt.Should().Contain("file contents");
    }

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_StopsAfterFiveIterations()
    {
        var (service, lm, sm, registry) = CreateServiceWithTools();
        sm.ActiveSession.WorkspacePath = @"C:\workspace";
        registry.Register(new FakeFileTool("read_file"));
        for (var i = 0; i < 6; i++)
            lm.ResponseQueue.Enqueue("<tool_call name=\"read_file\">{}</tool_call>");

        var response = await service.SendMessageAsync("Hello");

        lm.GenerateCallCount.Should().Be(5);
        response.Should().Be("<tool_call name=\"read_file\">{}</tool_call>");
    }

    [Fact]
    public async Task SendMessageAsync_ToolCallInResponse_UnknownToolReturnsErrorResult()
    {
        var (service, lm, sm, _) = CreateServiceWithTools();
        sm.ActiveSession.WorkspacePath = @"C:\workspace";
        lm.ResponseQueue.Enqueue("<tool_call name=\"no_such_tool\">{}</tool_call>");
        lm.ResponseQueue.Enqueue("Done");

        var response = await service.SendMessageAsync("Hello");

        lm.LastPrompt.Should().Contain("Unknown tool: no_such_tool");
        response.Should().Be("Done");
    }
}
