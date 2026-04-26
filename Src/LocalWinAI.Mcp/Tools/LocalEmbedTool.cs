using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool stub for text embedding. Requires a dedicated embedding model not yet available.</summary>
[McpServerToolType]
public sealed class LocalEmbedTool
{
    [McpServerTool(Name = "local_embed"), Description("Generate text embeddings locally. Not yet supported — requires a dedicated embedding model.")]
    public Task<string> EmbedAsync(
        [Description("The text to embed.")] string text,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Embedding is not yet supported. A dedicated embedding model is required.");
}
