using FluentAssertions;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Tests.Tools;

public sealed class ToolRegistryTests
{
    private static IFileTool MakeTool(string name) => new FakeFileTool(name);

    [Fact]
    public void TryGetTool_KnownName_ReturnsTrueAndTool()
    {
        var tool = MakeTool("list_directory");
        var registry = new ToolRegistry([tool]);

        var found = registry.TryGetTool("list_directory", out var result);

        found.Should().BeTrue();
        result.Should().BeSameAs(tool);
    }

    [Fact]
    public void TryGetTool_UnknownName_ReturnsFalse()
    {
        var registry = new ToolRegistry([MakeTool("list_directory")]);

        var found = registry.TryGetTool("unknown", out var result);

        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredTools()
    {
        var tools = new IFileTool[]
        {
            MakeTool("list_directory"),
            MakeTool("read_file"),
            MakeTool("search_in_file"),
        };
        var registry = new ToolRegistry(tools);

        registry.GetAll().Should().BeEquivalentTo(tools);
    }

    [Fact]
    public void Constructor_DuplicateName_Throws()
    {
        var act = () => new ToolRegistry([MakeTool("dupe"), MakeTool("dupe")]);

        act.Should().Throw<InvalidOperationException>().WithMessage("*dupe*");
    }

    [Fact]
    public void Constructor_EmptyCollection_BuildsEmptyRegistry()
    {
        var registry = new ToolRegistry([]);

        registry.GetAll().Should().BeEmpty();
    }

    private sealed class FakeFileTool(string name) : IFileTool
    {
        public string Name => name;
        public string Description => "fake";
        public Task<string> ExecuteAsync(IFileToolContext context, string argumentsJson, CancellationToken cancellationToken)
            => Task.FromResult("fake result");
    }
}
