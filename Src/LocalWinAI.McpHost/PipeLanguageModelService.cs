using System.IO.Pipes;
using System.Text.Json;
using LocalWinAI.Domain;
using LocalWinAI.Infrastructure.Pipe;

namespace LocalWinAI.McpHost;

internal sealed class PipeLanguageModelService : ILanguageModelService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<bool> EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        using var client = new NamedPipeClientStream(".", PipeConstants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(PipeConstants.ConnectTimeoutMs, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "LocalWinAI is not running. Please start the app before using local inference tools.", ex);
        }

        using var writer = new StreamWriter(client, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(client, leaveOpen: true);

        var request = new PipeRequest(Guid.NewGuid().ToString(), "ping");
        await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions));

        var line = await reader.ReadLineAsync(cancellationToken);
        if (line is null)
            throw new InvalidOperationException("LocalWinAI did not respond to ping.");

        return true;
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(PipeConstants.InferTimeoutMs);

        using var client = new NamedPipeClientStream(".", PipeConstants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(PipeConstants.ConnectTimeoutMs, cts.Token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "LocalWinAI is not running. Please start the app before using local inference tools.", ex);
        }

        using var writer = new StreamWriter(client, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(client, leaveOpen: true);

        var request = new PipeRequest(Guid.NewGuid().ToString(), "infer", prompt);
        await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions));

        var line = await reader.ReadLineAsync(cts.Token);
        if (line is null)
            throw new InvalidOperationException("LocalWinAI closed the connection without a response.");

        var response = JsonSerializer.Deserialize<PipeResponse>(line, JsonOptions)
            ?? throw new InvalidOperationException("LocalWinAI returned an invalid response.");

        if (response.Error is not null)
            throw new InvalidOperationException($"LocalWinAI inference error: {response.Error}");

        return response.Result ?? string.Empty;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(PipeConstants.InferTimeoutMs);

        using var client = new NamedPipeClientStream(".", PipeConstants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(PipeConstants.ConnectTimeoutMs, cts.Token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "LocalWinAI is not running. Please start the app before using local embedding tools.", ex);
        }

        using var writer = new StreamWriter(client, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(client, leaveOpen: true);

        var request = new PipeRequest(Guid.NewGuid().ToString(), "embed", text);
        await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions));

        var line = await reader.ReadLineAsync(cts.Token);
        if (line is null)
            throw new InvalidOperationException("LocalWinAI closed the connection without a response.");

        var response = JsonSerializer.Deserialize<PipeResponse>(line, JsonOptions)
            ?? throw new InvalidOperationException("LocalWinAI returned an invalid response.");

        if (response.Error is not null)
            throw new InvalidOperationException($"LocalWinAI embedding error: {response.Error}");

        return response.Embedding ?? throw new InvalidOperationException("LocalWinAI returned no embedding vector.");
    }
}
