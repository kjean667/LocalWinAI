namespace LocalWinAI.Application.Tools;

/// <summary>Formats tool results for consumption by the local model.</summary>
public static class ToolResultFormatter
{
    /// <summary>Wraps <paramref name="result"/> in a <c>tool_result</c> block addressed to <paramref name="toolName"/>.</summary>
    public static string Format(string toolName, string result) =>
        $"<tool_result name=\"{toolName}\">\n{result}\n</tool_result>";
}
