using System.Text.Json;
using LocalWinAI.Application.Statistics;
using LocalWinAI.Domain.Usage;

namespace LocalWinAI.Infrastructure.Usage;

public sealed class UsageAggregateService : IUsageAggregateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private volatile UsageAggregates _cached = UsageAggregates.Empty;
    private FileSystemWatcher? _watcher;
    private readonly System.Threading.Timer _pollingTimer;
    private long _lastKnownLength;
    private long _refreshPending;

    public event EventHandler? AggregatesChanged;

    public UsageAggregateService()
    {
        CompactIfNeeded();
        _cached = Compute();
        SetupWatcher();

        _pollingTimer = new System.Threading.Timer(
            _ => PollForChanges(),
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
    }

    public UsageAggregates GetAggregates() => _cached;

    private void PollForChanges()
    {
        if (!File.Exists(UsageTracker.FilePath)) return;
        var length = new FileInfo(UsageTracker.FilePath).Length;
        if (Interlocked.Read(ref _lastKnownLength) != length)
            ScheduleRefresh();
    }

    private void SetupWatcher()
    {
        var dir = Path.GetDirectoryName(UsageTracker.FilePath)!;
        if (!Directory.Exists(dir)) return;

        _watcher = new FileSystemWatcher(dir, Path.GetFileName(UsageTracker.FilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (_, _) => ScheduleRefresh();
        _watcher.Created += (_, _) => ScheduleRefresh();
    }

    // Debounce rapid file-change events (watcher can fire multiple times per write)
    private void ScheduleRefresh()
    {
        if (Interlocked.CompareExchange(ref _refreshPending, 1, 0) == 0)
        {
            Task.Delay(150).ContinueWith(_ =>
            {
                Interlocked.Exchange(ref _refreshPending, 0);
                DoRefresh();
            });
        }
    }

    private void DoRefresh()
    {
        _cached = Compute();
        if (File.Exists(UsageTracker.FilePath))
            Interlocked.Exchange(ref _lastKnownLength, new FileInfo(UsageTracker.FilePath).Length);
        AggregatesChanged?.Invoke(this, EventArgs.Empty);
    }

    private static UsageAggregates Compute()
    {
        if (!File.Exists(UsageTracker.FilePath))
            return UsageAggregates.Empty;

        var events = new List<UsageEvent>();
        try
        {
            using var stream = new FileStream(
                UsageTracker.FilePath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite, bufferSize: 65536);
            using var reader = new StreamReader(stream);

            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var evt = JsonSerializer.Deserialize<UsageEvent>(line, JsonOptions);
                    if (evt is not null)
                        events.Add(evt);
                }
                catch (JsonException) { }
            }
        }
        catch (IOException)
        {
            return UsageAggregates.Empty;
        }

        return Aggregate(events);
    }

    private static UsageAggregates Aggregate(List<UsageEvent> events)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.LocalDateTime);
        var sevenDaysAgo = today.AddDays(-6);

        var totalInput = events.Sum(e => (long)e.InputTokens);
        var totalOutput = events.Sum(e => (long)e.OutputTokens);

        var byTool = events
            .GroupBy(e => (e.Tool, e.Source))
            .Select(g => new ToolStats(
                g.Key.Tool,
                g.Key.Source,
                g.Count(),
                g.Count(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime) == today),
                g.Sum(e => (long)e.InputTokens),
                g.Sum(e => (long)e.OutputTokens)))
            .OrderByDescending(t => t.CallsAllTime)
            .ToList();

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-6 + i))
            .Select(d => new DayStats(
                d,
                events.Count(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime) == d)))
            .ToList();

        var activeDays = events
            .Select(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime))
            .ToHashSet();

        var streak = 0;
        var day = today;
        while (activeDays.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }

        return new UsageAggregates
        {
            TotalInputTokensAllTime = totalInput,
            TotalOutputTokensAllTime = totalOutput,
            TotalCallsAllTime = events.Count,
            TotalCallsToday = events.Count(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime) == today),
            TotalCallsLast7Days = events.Count(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime) >= sevenDaysAgo),
            CurrentStreakDays = streak,
            ByTool = byTool,
            Last7Days = last7Days
        };
    }

    private static void CompactIfNeeded()
    {
        if (!File.Exists(UsageTracker.FilePath)) return;

        try
        {
            var cutoff = DateTimeOffset.Now.AddDays(-90);
            var lines = File.ReadAllLines(UsageTracker.FilePath);
            var kept = new List<string>(lines.Length);
            var hadDrop = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var evt = JsonSerializer.Deserialize<UsageEvent>(line, JsonOptions);
                    if (evt is null || evt.Timestamp < cutoff)
                    {
                        hadDrop = true;
                        continue;
                    }
                }
                catch (JsonException)
                {
                    hadDrop = true;
                    continue;
                }
                kept.Add(line);
            }

            if (!hadDrop) return;

            var tmp = UsageTracker.FilePath + ".tmp";
            File.WriteAllLines(tmp, kept);
            File.Move(tmp, UsageTracker.FilePath, overwrite: true);
        }
        catch (IOException) { }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollingTimer.Dispose();
    }
}
