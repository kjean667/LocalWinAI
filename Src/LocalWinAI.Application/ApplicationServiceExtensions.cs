using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<ChatPageViewModel>();
        return services;
    }
}
