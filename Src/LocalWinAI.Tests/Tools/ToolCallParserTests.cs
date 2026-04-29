using FluentAssertions;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Tests.Tools;

public sealed class ToolCallParserTests
{
    [Fact]
    public void Parse_SingleToolCall_ExtractsNameAndArguments()
    {
        var text = """
            <tool_call name="read_file">
            {"path": "src/Program.cs"}
            </tool_call>
            """;

        var result = ToolCallParser.Parse(text);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("read_file");
        result[0].ArgumentsJson.Should().Be("""{"path": "src/Program.cs"}""");
    }

    [Fact]
    public void Parse_MultipleToolCalls_ExtractsAll()
    {
        var text = """
            <tool_call name="read_file">
            {"path": "a.cs"}
            </tool_call>
            <tool_call name="list_directory">
            {"path": "."}
            </tool_call>
            """;

        var result = ToolCallParser.Parse(text);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("read_file");
        result[0].ArgumentsJson.Should().Be("""{"path": "a.cs"}""");
        result[1].Name.Should().Be("list_directory");
        result[1].ArgumentsJson.Should().Be("""{"path": "."}""");
    }

    [Fact]
    public void Parse_NoToolCalls_ReturnsEmptyList()
    {
        var result = ToolCallParser.Parse("Just a regular assistant response.");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MalformedTag_ReturnsEmptyList()
    {
        var result = ToolCallParser.Parse("<tool_call>missing name attribute</tool_call>");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_UnclosedTag_ReturnsEmptyList()
    {
        var result = ToolCallParser.Parse("""<tool_call name="read_file">{"path":"a.cs"}""");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyList()
    {
        var result = ToolCallParser.Parse(string.Empty);

        result.Should().BeEmpty();
    }
}
