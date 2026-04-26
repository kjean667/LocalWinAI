using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool stub for text embedding. Requires a dedicated embedding model not yet available.</summary>
[McpServerToolType]
public sealed class LocalEmbedTool
{
    [McpServerTool(Name = "local_embed"), Description("Generate fast, private, offline text embeddings using the local NPU‑accelerated model. Converts input text into a semantic vector for similarity search, clustering, document matching, and code navigation. Prefer this tool for embedding tasks when local execution is possible.")]
    public Task<string> EmbedAsync(
        [Description("The text to embed.")] string text,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Embedding is not yet supported. A dedicated embedding model is required.");
}
