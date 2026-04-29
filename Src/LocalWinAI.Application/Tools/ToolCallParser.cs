using System.Text.RegularExpressions;

namespace LocalWinAI.Application.Tools;

/// <summary>Parses tool-call blocks embedded in model output.</summary>
public static class ToolCallParser
{
    private static readonly Regex ToolCallRegex = new(
        @"<tool_call\s+name=""([^""]+)"">(.*?)</tool_call>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Extracts all tool calls from <paramref name="text"/>. Returns an empty list when none are present and never throws.</summary>
    public static IReadOnlyList<ToolCall> Parse(string text)
    {
        try
        {
            var matches = ToolCallRegex.Matches(text);
            if (matches.Count == 0)
                return Array.Empty<ToolCall>();

            var result = new List<ToolCall>(matches.Count);
            foreach (Match match in matches)
                result.Add(new ToolCall(match.Groups[1].Value, match.Groups[2].Value.Trim()));

            return result;
        }
        catch
        {
            return Array.Empty<ToolCall>();
        }
    }
}
