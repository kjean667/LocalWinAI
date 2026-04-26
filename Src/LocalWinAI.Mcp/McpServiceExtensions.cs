using LocalWinAI.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Mcp;

public static class McpServiceExtensions
{
    /// <summary>Registers the MCP server with stdio transport and all local AI tools.</summary>
    public static IServiceCollection AddMcpServices(this IServiceCollection services)
    {
        services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<LocalInferTool>()
            .WithTools<LocalSummarizeTool>()
            .WithTools<LocalClassifyTool>()
            .WithTools<LocalEmbedTool>();

        return services;
    }
}
