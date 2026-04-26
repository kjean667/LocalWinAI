namespace LocalWinAI.Application.Statistics;

public sealed record ToolStats(
    string Tool,
    string Source,
    int CallsAllTime,
    int CallsToday,
    long InputTokensAllTime,
    long OutputTokensAllTime)
{
    public long TotalTokensAllTime => InputTokensAllTime + OutputTokensAllTime;
}

public sealed record DayStats(DateOnly Date, int Calls);

public sealed class UsageAggregates
{
    public static readonly UsageAggregates Empty = new();

    public long TotalInputTokensAllTime { get; init; }
    public long TotalOutputTokensAllTime { get; init; }
    public long TotalTokensAllTime => TotalInputTokensAllTime + TotalOutputTokensAllTime;

    public int TotalCallsAllTime { get; init; }
    public int TotalCallsToday { get; init; }
    public int TotalCallsLast7Days { get; init; }

    public int CurrentStreakDays { get; init; }

    public IReadOnlyList<ToolStats> ByTool { get; init; } = [];
    public IReadOnlyList<DayStats> Last7Days { get; init; } = [];

    // Approximate reference rates: $3/1M input tokens, $15/1M output tokens
    public decimal EstimatedInputCostSaved => (decimal)TotalInputTokensAllTime * 3m / 1_000_000m;
    public decimal EstimatedOutputCostSaved => (decimal)TotalOutputTokensAllTime * 15m / 1_000_000m;
    public decimal EstimatedTotalCostSaved => EstimatedInputCostSaved + EstimatedOutputCostSaved;
}
