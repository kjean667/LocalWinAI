using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LocalWinAI.Application.Statistics;

public sealed record ToolStatRow(
    string Name,
    string Icon,
    string Source,
    int CallsToday,
    int CallsAllTime,
    string TokensFormatted);

public sealed record DayBarRow(string DayLabel, int Calls, double BarHeight);

public partial class StatisticsPageViewModel : ObservableObject
{
    private readonly IUsageAggregateService _service;
    private readonly SynchronizationContext? _syncContext;

    [ObservableProperty]
    public partial string TotalTokensFormatted { get; set; } = "0";

    [ObservableProperty]
    public partial string EstimatedSavingsFormatted { get; set; } = "$0.00";

    [ObservableProperty]
    public partial string StreakText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasStreak { get; set; }

    [ObservableProperty]
    public partial string TodaySummary { get; set; } = "No activity today";

    [ObservableProperty]
    public partial string LastRefreshedText { get; set; } = string.Empty;

    public ObservableCollection<ToolStatRow> ToolStats { get; } = [];
    public ObservableCollection<DayBarRow> DayBars { get; } = [];

    public IRelayCommand RefreshCommand { get; }

    public StatisticsPageViewModel(IUsageAggregateService service)
    {
        _service = service;
        _syncContext = SynchronizationContext.Current;
        _service.AggregatesChanged += OnAggregatesChanged;
        RefreshCommand = new RelayCommand(Refresh);
        Refresh();
    }

    private void OnAggregatesChanged(object? sender, EventArgs e)
    {
        if (_syncContext is not null)
            _syncContext.Post(_ => Refresh(), null);
        else
            Refresh();
    }

    public void Refresh()
    {
        UpdateFrom(_service.GetAggregates());
    }

    private void UpdateFrom(UsageAggregates agg)
    {
        TotalTokensFormatted = FormatNumber(agg.TotalTokensAllTime);
        EstimatedSavingsFormatted = $"${agg.EstimatedTotalCostSaved:F2}";

        HasStreak = agg.CurrentStreakDays >= 2;
        StreakText = HasStreak ? $"{agg.CurrentStreakDays}-day streak" : string.Empty;

        TodaySummary = agg.TotalCallsToday == 0
            ? "No activity today"
            : $"{agg.TotalCallsToday} call{(agg.TotalCallsToday != 1 ? "s" : "")} today";

        LastRefreshedText = DateTime.Now.ToString("HH:mm:ss");

        UpdateToolStats(agg);
        UpdateDayBars(agg);
    }

    private void UpdateToolStats(UsageAggregates agg)
    {
        ToolStats.Clear();
        foreach (var t in agg.ByTool)
        {
            ToolStats.Add(new ToolStatRow(
                ToolDisplayName(t.Tool),
                ToolIcon(t.Tool),
                t.Source == "mcp" ? "MCP" : "Chat UI",
                t.CallsToday,
                t.CallsAllTime,
                FormatNumber(t.TotalTokensAllTime)));
        }
    }

    private void UpdateDayBars(UsageAggregates agg)
    {
        DayBars.Clear();
        var maxCalls = agg.Last7Days.Count > 0 ? agg.Last7Days.Max(d => d.Calls) : 0;
        foreach (var day in agg.Last7Days)
        {
            var height = maxCalls > 0 ? (double)day.Calls / maxCalls * 56.0 : 0;
            DayBars.Add(new DayBarRow(
                day.Date.ToString("ddd"),
                day.Calls,
                day.Calls > 0 ? Math.Max(height, 4.0) : 0.0));
        }
    }

    private static string ToolDisplayName(string tool) => tool switch
    {
        "chat" => "Chat",
        "local_infer" => "Infer",
        "local_summarize" => "Summarize",
        "local_classify" => "Classify",
        "local_embed" => "Embed",
        _ => tool
    };

    // Segoe MDL2 Assets glyph codes (char casts keep source file pure ASCII)
    private static string ToolIcon(string tool) => tool switch
    {
        "chat" => ((char)0xE8BD).ToString(),           // Comment
        "local_infer" => ((char)0xE9F9).ToString(),    // Processing
        "local_summarize" => ((char)0xE8D2).ToString(), // ReadingList
        "local_classify" => ((char)0xE71C).ToString(),  // Filter
        "local_embed" => ((char)0xE721).ToString(),    // Search
        _ => ((char)0xE8A1).ToString()
    };


    private static string FormatNumber(long n) => n switch
    {
        >= 1_000_000 => $"{n / 1_000_000.0:F1}M",
        >= 10_000 => $"{n / 1_000.0:F0}K",
        >= 1_000 => $"{n / 1_000.0:F1}K",
        _ => n.ToString("N0")
    };
}
