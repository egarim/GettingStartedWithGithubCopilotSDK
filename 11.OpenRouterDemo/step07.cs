#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 7: Streaming con modelo custom
using System.Text;
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

var chosenModel = "gpt-4o";
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel,
    Streaming = true  // habilitar streaming con modelo BYOK
});

var sb = new StringBuilder();
var idleTcs = new TaskCompletionSource<bool>();

session.On(evt =>
{
    if (evt is AssistantMessageDeltaEvent delta)
    {
        Console.Write(delta.Data.DeltaContent);
        sb.Append(delta.Data.DeltaContent);
    }
    if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
});

Console.Write("  Streaming: ");
await session.SendAsync(
    new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
Console.WriteLine($"\n  Total chars: {sb.Length}");

await client.StopAsync();
await client.DisposeAsync();
