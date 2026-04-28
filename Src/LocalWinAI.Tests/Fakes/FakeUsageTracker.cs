using LocalWinAI.Domain.Usage;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeUsageTracker : IUsageTracker
{
    public List<UsageEvent> RecordedEvents { get; } = [];

    public Task RecordAsync(UsageEvent evt)
    {
        RecordedEvents.Add(evt);
        return Task.CompletedTask;
    }
}
