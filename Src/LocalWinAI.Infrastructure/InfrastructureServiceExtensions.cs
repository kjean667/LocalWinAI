using LocalWinAI.Application.Settings;
using LocalWinAI.Domain;
using LocalWinAI.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILanguageModelService, WindowsLanguageModelService>();
        services.AddSingleton<IClaudeCodeSettingsService, ClaudeCodeSettingsService>();
        return services;
    }
}
