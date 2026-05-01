using LocalWinAI.Application;
using LocalWinAI.Application.Settings;
using LocalWinAI.Application.Workspaces;
using LocalWinAI.Infrastructure;
using LocalWinAI.Infrastructure.Pipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace LocalWinAI;

// 'Application' is disambiguated with global:: because LocalWinAI.Application is also a namespace in this project.
public partial class App : global::Microsoft.UI.Xaml.Application
{
    /// <summary>Application-wide service provider. Available after the constructor runs.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();
        Services = BuildServiceProvider();
        StartPipeServer();
        LoadWorkspaces();
    }

    /// <summary>The application's main window. Available after <see cref="OnLaunched"/> runs.</summary>
    public static Window AppWindow { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        AppWindow = _window;
        _window.Activate();
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddDebug());
        services.AddInfrastructureServices();
        services.AddApplicationServices();
        services.AddSingleton<IMcpProviderDescriptor, LocalWinAiMcpDescriptor>();
        return services.BuildServiceProvider();
    }

    private static void StartPipeServer()
    {
        var server = Services.GetRequiredService<NamedPipeInferenceServer>();
        // StartAsync is fast (just kicks off the background loop); fire-and-forget is intentional.
        _ = server.StartAsync(CancellationToken.None);
    }

    private static void LoadWorkspaces()
    {
        var manager = Services.GetRequiredService<IWorkspaceManager>();
        _ = manager.LoadAsync(CancellationToken.None);
    }

    private Window? _window;
}
