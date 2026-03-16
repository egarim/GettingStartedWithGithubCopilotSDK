#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Boilerplate - crear cliente y helpers
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});

Console.WriteLine("02 - DEMO: Ciclo de vida, eventos y multi-turno");

await client.DisposeAsync();
