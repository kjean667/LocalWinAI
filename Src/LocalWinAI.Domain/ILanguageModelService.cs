namespace LocalWinAI.Domain;

/// <summary>Abstracts access to a local language model for text generation.</summary>
public interface ILanguageModelService
{
    /// <summary>Ensures the language model is ready to use. Returns true on success.</summary>
    Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default);

    /// <summary>Generates a text response for the given prompt.</summary>
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
}
