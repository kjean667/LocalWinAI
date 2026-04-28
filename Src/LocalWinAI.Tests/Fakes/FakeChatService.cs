using LocalWinAI.Application;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeChatService : IChatService
{
    public string Response { get; set; } = "Fake response";
    public bool ThrowOnSend { get; set; } = false;
    public int SendCallCount { get; private set; }

    public Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        SendCallCount++;
        if (ThrowOnSend)
            throw new InvalidOperationException("Language model is not ready.");
        return Task.FromResult(Response);
    }
}
