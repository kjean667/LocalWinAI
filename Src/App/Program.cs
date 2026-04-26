using LocalWinAI.Infrastructure;
using LocalWinAI.Mcp;
using Microsoft.Extensions.Hosting;

namespace LocalWinAI;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Contains("--mcp"))
        {
            RunMcpModeAsync(args).GetAwaiter().GetResult();
            return;
        }

        global::WinRT.ComWrappersSupport.InitializeComWrappers();
        global::Microsoft.UI.Xaml.Application.Start(_ =>
        {
            var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }

    private static async Task RunMcpModeAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddInfrastructureServices();
        builder.Services.AddMcpServices();
        await builder.Build().RunAsync();
    }
}
