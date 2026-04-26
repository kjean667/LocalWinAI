namespace LocalWinAI.Infrastructure.Pipe;

/// <param name="Id">Correlation ID from the request.</param>
/// <param name="Result">Non-null on success.</param>
/// <param name="Error">Non-null on failure.</param>
public sealed record PipeResponse(string Id, string? Result, string? Error = null);
