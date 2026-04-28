using LocalWinAI.Domain;
using LocalWinAI.Infrastructure;
using LocalWinAI.McpHost;
using LocalWinAI.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoreInfrastructureServices();
builder.Services.AddSingleton<ILanguageModelService, PipeLanguageModelService>();
builder.Services.AddMcpServices();
await builder.Build().RunAsync();
