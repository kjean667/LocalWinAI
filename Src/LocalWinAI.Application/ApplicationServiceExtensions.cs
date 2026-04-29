using LocalWinAI.Application.Sessions;
using LocalWinAI.Application.Settings;
using LocalWinAI.Application.Statistics;
using LocalWinAI.Application.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LocalWinAI.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatSessionManager, ChatSessionManager>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<ChatPageViewModel>();
        services.AddSingleton<SettingsPageViewModel>();
        services.AddSingleton<StatisticsPageViewModel>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        return services;
    }
}
