namespace LocalWinAI.Domain;

/// <summary>Abstracts access to a local language model for text generation.</summary>
public interface ILanguageModelService
{
    /// <summary>Ensures the language model is ready to use. Returns true on success.</summary>
    Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default);

    /// <summary>Generates a text response for the given prompt.</summary>
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously generates a vector embedding representation for the specified text.
    /// </summary>
    /// <param name="text">The input text to generate an embedding for. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of single-precision
    /// floating-point values representing the embedding of the input text.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct);
}
