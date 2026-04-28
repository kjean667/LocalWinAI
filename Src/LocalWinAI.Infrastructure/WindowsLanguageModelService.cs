using LocalWinAI.Domain;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;

namespace LocalWinAI.Infrastructure;

/// <summary>Implements <see cref="ILanguageModelService"/> using the Windows Copilot Runtime on-device language model.</summary>
public sealed class WindowsLanguageModelService : ILanguageModelService, IDisposable
{
    private LanguageModel? _languageModel;

    public async Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var readyState = LanguageModel.GetReadyState();
            if (readyState == AIFeatureReadyState.Ready)
                return true;

            var op = await LanguageModel.EnsureReadyAsync();
            if (op.Status == AIFeatureReadyResultState.Success)
                return true;

            throw new InvalidOperationException(
                $"Language model not ready. GetReadyState={readyState}, EnsureReadyAsync={op.Status}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Language model initialization failed: {ex.GetType().Name} 0x{ex.HResult:X8}: {ex.Message}", ex);
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _languageModel ??= await LanguageModel.CreateAsync();
        var result = await _languageModel.GenerateResponseAsync(prompt);
        return result.Text;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty.", nameof(text));

        _languageModel ??= await LanguageModel.CreateAsync();

        var result = _languageModel.GenerateEmbeddingVectors(text);

        if (result?.EmbeddingVectors is null || result.EmbeddingVectors.Count == 0)
            throw new InvalidOperationException("Embedding model returned null or empty result.");

        var dim = result.EmbeddingVectors[0].Size;
        float[] pooled = new float[dim];

        for (int i = 0; i < result.EmbeddingVectors.Count; i++)
        {
            float[] vec = new float[dim];
            result.EmbeddingVectors[i].GetValues(vec);
            for (int j = 0; j < dim; j++)
                pooled[j] += vec[j];
        }

        int count = result.EmbeddingVectors.Count;
        for (int j = 0; j < dim; j++)
            pooled[j] /= count;

        return pooled;
    }

    public void Dispose() => _languageModel?.Dispose();
}
