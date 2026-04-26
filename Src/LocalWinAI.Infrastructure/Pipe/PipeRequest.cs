namespace LocalWinAI.Infrastructure.Pipe;

/// <param name="Id">Correlation ID echoed back in the response.</param>
/// <param name="Method">"ping" or "infer".</param>
/// <param name="Prompt">Required when Method is "infer".</param>
public sealed record PipeRequest(string Id, string Method, string? Prompt = null);
