using System.IO.Pipes;
using System.Text.Json;
using LocalWinAI.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocalWinAI.Infrastructure.Pipe;

public sealed class NamedPipeInferenceServer : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILanguageModelService _languageModel;
    private readonly ILogger<NamedPipeInferenceServer> _logger;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;

    public NamedPipeInferenceServer(
        ILanguageModelService languageModel,
        ILogger<NamedPipeInferenceServer> logger)
    {
        _languageModel = languageModel;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        _acceptLoop = AcceptLoopAsync(_cts.Token);
        _logger.LogInformation("Named pipe inference server started on {PipeName}", PipeConstants.PipeName);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        if (_acceptLoop is not null)
        {
            try { await _acceptLoop.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var pipe = new NamedPipeServerStream(
                PipeConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await pipe.WaitForConnectionAsync(cancellationToken);
                _ = HandleConnectionAsync(pipe, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await pipe.DisposeAsync();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error accepting pipe connection");
                await pipe.DisposeAsync();
            }
        }
    }

    private async Task HandleConnectionAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        await using (pipe)
        {
            try
            {
                using var reader = new StreamReader(pipe, leaveOpen: true);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };

                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                    return;

                var request = JsonSerializer.Deserialize<PipeRequest>(line, JsonOptions);
                if (request is null)
                    return;

                PipeResponse response;
                if (request.Method == "ping")
                {
                    response = new PipeResponse(request.Id, "pong");
                }
                else if (request.Method == "embed")
                {
                    try
                    {
                        var embedding = await _languageModel.GenerateEmbeddingAsync(
                            request.Prompt ?? string.Empty, cancellationToken);
                        response = new PipeResponse(request.Id, null, Embedding: embedding);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Embedding failed for request {Id}", request.Id);
                        response = new PipeResponse(request.Id, null, ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        var result = await _languageModel.GenerateResponseAsync(
                            request.Prompt ?? string.Empty, cancellationToken);
                        response = new PipeResponse(request.Id, result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Inference failed for request {Id}", request.Id);
                        response = new PipeResponse(request.Id, null, ex.Message);
                    }
                }

                await writer.WriteLineAsync(JsonSerializer.Serialize(response, JsonOptions));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pipe connection closed unexpectedly");
            }
        }
    }
}
