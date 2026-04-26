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
        if (LanguageModel.GetReadyState() == AIFeatureReadyState.Ready)
            return true;

        var op = await LanguageModel.EnsureReadyAsync();
        return op.Status == AIFeatureReadyResultState.Success;
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _languageModel ??= await LanguageModel.CreateAsync();
        var result = await _languageModel.GenerateResponseAsync(prompt);
        return result.Text;
    }

    public void Dispose() => _languageModel?.Dispose();
}
