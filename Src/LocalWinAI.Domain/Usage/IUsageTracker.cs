namespace LocalWinAI.Domain.Usage;

public interface IUsageTracker
{
    Task RecordAsync(UsageEvent evt);
}
