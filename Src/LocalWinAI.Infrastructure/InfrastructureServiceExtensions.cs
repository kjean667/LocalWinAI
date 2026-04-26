using LocalWinAI.Application.Settings;
using LocalWinAI.Application.Statistics;
using LocalWinAI.Domain;
using LocalWinAI.Domain.Usage;
using LocalWinAI.Infrastructure.Settings;
using LocalWinAI.Infrastructure.Usage;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILanguageModelService, WindowsLanguageModelService>();
        services.AddSingleton<IClaudeCodeSettingsService, ClaudeCodeSettingsService>();
        services.AddSingleton<IUsageTracker, UsageTracker>();
        services.AddSingleton<IUsageAggregateService, UsageAggregateService>();
        return services;
    }
}
