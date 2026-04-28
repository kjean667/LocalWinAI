using LocalWinAI.Application.Settings;

namespace LocalWinAI;

internal sealed class LocalWinAiMcpDescriptor : IMcpProviderDescriptor
{
    public string ServerKey => "localwinai";
    public string DisplayName => "LocalWinAI";
    public string Description => "Run prompts through the local NPU-accelerated language model without consuming cloud API tokens.";
    public string Command { get; } = Environment.ProcessPath is { } p
        ? Path.Combine(Path.GetDirectoryName(p) ?? string.Empty, "LocalWinAI.McpHost.exe")
        : string.Empty;
    public string[] Args { get; } = ["--mcp"];
}
