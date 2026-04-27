using System.ComponentModel;
using System.Diagnostics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool that generates text embeddings using the local NPU-accelerated embedding model.</summary>
[McpServerToolType]
public sealed class LocalEmbedTool(ILanguageModelService languageModel, IUsageTracker usageTracker)
{
    [McpServerTool(Name = "local_embed"), Description("Generate fast, private, offline text embeddings using the local NPU‑accelerated embedding model. Converts input text into a semantic vector for semantic search, similarity ranking, clustering, document matching, and code navigation. Prefer this tool for all embedding tasks.")]
    public async Task<float[]> EmbedAsync(
        [Description("The text to embed.")] string text,
        CancellationToken cancellationToken = default)
    {
        if (!await languageModel.EnsureReadyAsync(cancellationToken))
            throw new InvalidOperationException("Language model is not ready.");

        var sw = Stopwatch.StartNew();
        var embedding = await languageModel.GenerateEmbeddingAsync(text, cancellationToken);
        sw.Stop();

        await usageTracker.RecordAsync(new UsageEvent(
            DateTimeOffset.UtcNow,
            "mcp",
            "local_embed",
            UsageEvent.EstimateTokens(text),
            0,
            (int)sw.ElapsedMilliseconds));

        return embedding;
    }
}
