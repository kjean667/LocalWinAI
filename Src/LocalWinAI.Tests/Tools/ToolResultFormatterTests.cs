using FluentAssertions;
using LocalWinAI.Application.Tools;

namespace LocalWinAI.Tests.Tools;

public sealed class ToolResultFormatterTests
{
    [Fact]
    public void Format_ProducesCorrectXmlWrapper()
    {
        var result = ToolResultFormatter.Format("read_file", "file contents here");

        result.Should().Be("<tool_result name=\"read_file\">\nfile contents here\n</tool_result>");
    }
}
