using LocalWinAI.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILanguageModelService, WindowsLanguageModelService>();
        return services;
    }
}
