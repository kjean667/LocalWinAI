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

    public void Dispose() => _languageModel?.Dispose();
}
