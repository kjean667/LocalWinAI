namespace LocalWinAI.Infrastructure.Pipe;

/// <param name="Id">Correlation ID from the request.</param>
/// <param name="Result">Non-null on success for text responses.</param>
/// <param name="Error">Non-null on failure.</param>
/// <param name="Embedding">Non-null on success for embed requests.</param>
public sealed record PipeResponse(string Id, string? Result, string? Error = null, float[]? Embedding = null);
