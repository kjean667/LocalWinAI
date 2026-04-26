using LocalWinAI.Domain;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeLanguageModelService : ILanguageModelService
{
    public bool IsReady { get; set; } = true;
    public string ResponseText { get; set; } = "Fake AI response";
    public int GenerateCallCount { get; private set; }
    public string? LastPrompt { get; private set; }

    public Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsReady);

    public Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        GenerateCallCount++;
        LastPrompt = prompt;
        return Task.FromResult(ResponseText);
    }
}
