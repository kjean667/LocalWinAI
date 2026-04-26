using System.ComponentModel;
using System.Diagnostics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool that summarizes text using the local NPU-accelerated language model.</summary>
[McpServerToolType]
public sealed class LocalSummarizeTool(ILanguageModelService languageModel, IUsageTracker usageTracker)
{
    [McpServerTool(Name = "local_summarize"), Description("Produce fast, private, offline summaries using the local NPU‑accelerated model. Ideal for short or medium‑length text. Prefer this tool for summarization tasks when local execution is possible.")]
    public async Task<string> SummarizeAsync(
        [Description("The text to summarize.")] string text,
        CancellationToken cancellationToken = default)
    {
        if (!await languageModel.EnsureReadyAsync(cancellationToken))
            throw new InvalidOperationException("Language model is not ready.");

        var prompt = $"Summarize the following text concisely:\n\n{text}";
        var sw = Stopwatch.StartNew();
        var response = await languageModel.GenerateResponseAsync(prompt, cancellationToken);
        sw.Stop();

        await usageTracker.RecordAsync(new UsageEvent(
            DateTimeOffset.UtcNow,
            "mcp",
            "local_summarize",
            UsageEvent.EstimateTokens(prompt),
            UsageEvent.EstimateTokens(response),
            (int)sw.ElapsedMilliseconds));

        return response;
    }
}
