#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Compactacion desactivada - Sin eventos
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

var compactionEvents = new List<SessionEvent>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig { Enabled = false }
});

session.On(evt =>
{
    if (evt is SessionCompactionStartEvent or SessionCompactionCompleteEvent)
        compactionEvents.Add(evt);
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  Eventos compactacion: {compactionEvents.Count}"); // Esperado: 0

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
