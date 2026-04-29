using LocalWinAI.Domain;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeLanguageModelService : ILanguageModelService
{
    public bool IsReady { get; set; } = true;
    public string ResponseText { get; set; } = "Fake AI response";
    public Queue<string> ResponseQueue { get; } = new();
    public float[] EmbeddingVector { get; set; } = [0.1f, 0.2f, 0.3f];
    public int GenerateCallCount { get; private set; }
    public string? LastPrompt { get; private set; }
    public string? LastEmbedText { get; private set; }

    public Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsReady);

    public Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        GenerateCallCount++;
        LastPrompt = prompt;
        var text = ResponseQueue.Count > 0 ? ResponseQueue.Dequeue() : ResponseText;
        return Task.FromResult(text);
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        LastEmbedText = text;
        return Task.FromResult(EmbeddingVector);
    }
}
