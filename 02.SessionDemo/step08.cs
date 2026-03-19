#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 8: Reanudar sesion inexistente (manejo de errores)
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

try
{
    await client.ResumeSessionAsync("non-existent-session-id");
}
catch (IOException ex)
{
    Console.WriteLine($"Error esperado: {ex.Message}"); // -> IOException
}

await client.StopAsync();
await client.DisposeAsync();
