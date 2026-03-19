#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Suscripcion a eventos con session.On
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

await using var session = await client.CreateSessionAsync();
var receivedEvents = new List<string>();
var idleTcs = new TaskCompletionSource<bool>();

var sub = session.On(evt =>
{
    receivedEvents.Add(evt.GetType().Name);
    if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
});

await session.SendAsync(new MessageOptions { Prompt = "What is 100 + 200?" });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
sub.Dispose();

foreach (var e in receivedEvents)
    Console.WriteLine($"  {e}"); // -> AssistantMessageEvent, SessionIdleEvent, etc.

await client.StopAsync();
await client.DisposeAsync();
