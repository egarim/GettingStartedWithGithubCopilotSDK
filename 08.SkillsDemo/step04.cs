#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Sin skill (comparacion de linea base)
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

var session = await client.CreateSessionAsync(new SessionConfig());  // sin SkillDirectories

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly."
});
var containsMarker = answer?.Data.Content?.Contains("PINEAPPLE_COCONUT_42") ?? false;
Console.WriteLine($"  Respuesta (sin skill): {answer?.Data.Content}");
Console.WriteLine($"  Contiene marcador: {containsMarker}"); // Esperado: False

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
