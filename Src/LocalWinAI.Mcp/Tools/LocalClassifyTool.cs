using System.ComponentModel;
using LocalWinAI.Domain;
using ModelContextProtocol.Server;

namespace LocalWinAI.Mcp.Tools;

/// <summary>MCP tool that classifies text into one of the provided categories using the local NPU-accelerated language model.</summary>
[McpServerToolType]
public sealed class LocalClassifyTool(ILanguageModelService languageModel)
{
    [McpServerTool(Name = "local_classify"), Description("Perform fast, offline text classification using the local NPU‑accelerated model. Given a list of categories, return the best match with a confidence score. Prefer this tool for lightweight classification tasks.")]
    public async Task<string> ClassifyAsync(
        [Description("The text to classify.")] string text,
        [Description("Comma-separated list of categories to classify into.")] string categories,
        CancellationToken cancellationToken = default)
    {
        if (!await languageModel.EnsureReadyAsync(cancellationToken))
            throw new InvalidOperationException("Language model is not ready.");

        var prompt = $"Classify the following text into exactly one of these categories: {categories}\n\nText: {text}\n\nRespond with only the category name.";
        return await languageModel.GenerateResponseAsync(prompt, cancellationToken);
    }
}
