namespace LocalWinAI.Application.Statistics;

public interface IUsageAggregateService : IDisposable
{
    UsageAggregates GetAggregates();
    event EventHandler? AggregatesChanged;
}
