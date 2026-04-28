using System.ComponentModel;
using System.Diagnostics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool that runs a prompt through the local NPU-accelerated language model.</summary>
[McpServerToolType]
public sealed class LocalInferTool(ILanguageModelService languageModel, IUsageTracker usageTracker)
{
    [McpServerTool(Name = "local_infer"), Description("Run a prompt through the local NPU‑accelerated model for quick, deterministic, offline inference. Best for small reasoning tasks, transformations, rewrites, or structured outputs that do not require full Claude‑level reasoning.")]
    public async Task<string> InferAsync(
        [Description("The prompt to send to the local model.")] string prompt,
        CancellationToken cancellationToken = default)
    {
        if (!await languageModel.EnsureReadyAsync(cancellationToken))
            throw new InvalidOperationException("Language model is not ready.");

        var sw = Stopwatch.StartNew();
        var response = await languageModel.GenerateResponseAsync(prompt, cancellationToken);
        sw.Stop();

        await usageTracker.RecordAsync(new UsageEvent(
            DateTimeOffset.UtcNow,
            "mcp",
            "local_infer",
            UsageEvent.EstimateTokens(prompt),
            UsageEvent.EstimateTokens(response),
            (int)sw.ElapsedMilliseconds));

        return response;
    }
}
