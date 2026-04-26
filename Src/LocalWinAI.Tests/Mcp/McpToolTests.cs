using FluentAssertions;
using LocalWinAI.Mcp.Tools;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests.Mcp;

public sealed class McpToolTests
{
    [Fact]
    public async Task InferAsync_WhenModelReady_ReturnsGeneratedResponse()
    {
        var fake = new FakeLanguageModelService { ResponseText = "Hello from NPU" };
        var tool = new LocalInferTool(fake, new FakeUsageTracker());

        var result = await tool.InferAsync("say hello");

        result.Should().Be("Hello from NPU");
        fake.LastPrompt.Should().Be("say hello");
    }

    [Fact]
    public async Task InferAsync_WhenModelNotReady_ThrowsInvalidOperationException()
    {
        var fake = new FakeLanguageModelService { IsReady = false };
        var tool = new LocalInferTool(fake, new FakeUsageTracker());

        var act = async () => await tool.InferAsync("prompt");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SummarizeAsync_WhenModelReady_ReturnsSummaryResponse()
    {
        var fake = new FakeLanguageModelService { ResponseText = "A short summary." };
        var tool = new LocalSummarizeTool(fake, new FakeUsageTracker());

        var result = await tool.SummarizeAsync("A very long piece of text that needs summarizing.");

        result.Should().Be("A short summary.");
        fake.LastPrompt.Should().Contain("A very long piece of text that needs summarizing.");
        fake.LastPrompt.Should().Contain("Summarize");
    }

    [Fact]
    public async Task SummarizeAsync_WhenModelNotReady_ThrowsInvalidOperationException()
    {
        var fake = new FakeLanguageModelService { IsReady = false };
        var tool = new LocalSummarizeTool(fake, new FakeUsageTracker());

        var act = async () => await tool.SummarizeAsync("text");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ClassifyAsync_WhenModelReady_IncludesTextAndCategoriesInPrompt()
    {
        var fake = new FakeLanguageModelService { ResponseText = "spam" };
        var tool = new LocalClassifyTool(fake, new FakeUsageTracker());

        var result = await tool.ClassifyAsync("Win a free prize!", "spam,not spam");

        result.Should().Be("spam");
        fake.LastPrompt.Should().Contain("Win a free prize!");
        fake.LastPrompt.Should().Contain("spam,not spam");
    }

    [Fact]
    public async Task ClassifyAsync_WhenModelNotReady_ThrowsInvalidOperationException()
    {
        var fake = new FakeLanguageModelService { IsReady = false };
        var tool = new LocalClassifyTool(fake, new FakeUsageTracker());

        var act = async () => await tool.ClassifyAsync("text", "a,b");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EmbedAsync_WhenCalled_ThrowsNotSupportedException()
    {
        var tool = new LocalEmbedTool();

        var act = async () => await tool.EmbedAsync("some text");

        await act.Should().ThrowAsync<NotSupportedException>();
    }
}
