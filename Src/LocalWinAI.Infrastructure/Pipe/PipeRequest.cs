namespace LocalWinAI.Infrastructure.Pipe;

/// <param name="Id">Correlation ID echoed back in the response.</param>
/// <param name="Method">"ping", "infer", or "embed".</param>
/// <param name="Prompt">Required when Method is "infer" or "embed".</param>
public sealed record PipeRequest(string Id, string Method, string? Prompt = null);
