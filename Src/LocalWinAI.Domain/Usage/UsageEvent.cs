namespace LocalWinAI.Domain.Usage;

public sealed record UsageEvent(
    DateTimeOffset Timestamp,
    string Source,      // "chat" | "mcp"
    string Tool,        // "chat" | "local_infer" | "local_summarize" | "local_classify"
    int InputTokens,
    int OutputTokens,
    int DurationMs)
{
    public static int EstimateTokens(string text) => Math.Max(1, text.Length / 4);
}
