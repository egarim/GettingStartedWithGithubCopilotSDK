#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 2: Listar todos los modelos (built-in + BYOK)
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

var models = await client.ListModelsAsync();
Console.WriteLine($"  Total modelos: {models.Count}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-45} {m.Name}");
// Los modelos custom de BYOK aparecen en esta lista!

await client.StopAsync();
await client.DisposeAsync();
