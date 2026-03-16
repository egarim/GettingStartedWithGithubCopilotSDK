#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Iniciar cliente para demo MCP y agentes
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();
Console.WriteLine("09 - DEMO: Servidores MCP y agentes personalizados");

await client.StopAsync();
await client.DisposeAsync();
