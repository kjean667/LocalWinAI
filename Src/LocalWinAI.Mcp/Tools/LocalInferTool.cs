using System.ComponentModel;
using LocalWinAI.Domain;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool that runs a prompt through the local NPU-accelerated language model.</summary>
[McpServerToolType]
public sealed class LocalInferTool(ILanguageModelService languageModel)
{
    [McpServerTool(Name = "local_infer"), Description("Run a prompt through the local NPU-accelerated language model.")]
    public async Task<string> InferAsync(
        [Description("The prompt to send to the local model.")] string prompt,
        CancellationToken cancellationToken = default)
    {
        if (!await languageModel.EnsureReadyAsync(cancellationToken))
            throw new InvalidOperationException("Language model is not ready.");

        return await languageModel.GenerateResponseAsync(prompt, cancellationToken);
    }
}
