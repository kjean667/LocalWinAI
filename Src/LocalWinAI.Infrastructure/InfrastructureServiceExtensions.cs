using LocalWinAI.Application.Settings;
using LocalWinAI.Application.Statistics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;
using LocalWinAI.Infrastructure.Pipe;
using LocalWinAI.Infrastructure.Settings;
using LocalWinAI.Infrastructure.Usage;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers services that are safe to use from any process (no Windows App Runtime required).
    /// Call this from both the UI app and McpHost.
    /// </summary>
    public static IServiceCollection AddCoreInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IClaudeCodeSettingsService, ClaudeCodeSettingsService>();
        services.AddSingleton<IUsageTracker, UsageTracker>();
        services.AddSingleton<IUsageAggregateService, UsageAggregateService>();
        return services;
    }

    /// <summary>
    /// Registers the Windows Copilot Runtime language model. Requires a packaged app with LAF permissions.
    /// Call this only from the UI app.
    /// </summary>
    public static IServiceCollection AddWindowsAiServices(this IServiceCollection services)
    {
        services.AddSingleton<ILanguageModelService, WindowsLanguageModelService>();
        return services;
    }

    /// <summary>
    /// Registers the named pipe inference server as a hosted service.
    /// Call this only from the UI app after <see cref="AddWindowsAiServices"/>.
    /// </summary>
    public static IServiceCollection AddPipeInferenceServer(this IServiceCollection services)
    {
        services.AddSingleton<NamedPipeInferenceServer>();
        return services;
    }

    /// <summary>Convenience method that registers all infrastructure services for the UI app.</summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddCoreInfrastructureServices();
        services.AddWindowsAiServices();
        services.AddPipeInferenceServer();
        return services;
    }
}
