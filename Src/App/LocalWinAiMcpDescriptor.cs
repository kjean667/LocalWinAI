using LocalWinAI.Application.Settings;

namespace LocalWinAI;

internal sealed class LocalWinAiMcpDescriptor : IMcpProviderDescriptor
{
    public string ServerKey => "localwinai";
    public string DisplayName => "LocalWinAI";
    public string Description => "Run prompts through the local NPU-accelerated language model without consuming cloud API tokens.";
    public string Command { get; } = Environment.ProcessPath?.Replace(".exe", ".McpHost.exe") ?? string.Empty;
    public string[] Args { get; } = ["--mcp"];
}
