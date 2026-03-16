#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 6: SendAndWaitAsync (bloquea hasta idle)
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

await using var session = await client.CreateSessionAsync();
var events = new List<string>();
session.On(evt => events.Add(evt.Type));

var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
Console.WriteLine($"Respuesta: {response?.Data.Content}"); // -> 6
Console.WriteLine($"Eventos: {string.Join(", ", events.Distinct())}");
// -> session.idle YA esta en events (SendAndWaitAsync bloquea hasta idle)

await client.StopAsync();
await client.DisposeAsync();
