#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 8: Chat interactivo con modelo BYOK
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
    Streaming = true
});

Console.WriteLine($"Chat interactivo con {chosenModel} (vacio para salir):\n");
while (true)
{
    Console.Write("  Tu: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var sb = new StringBuilder();
    var idleTcs = new TaskCompletionSource<bool>();
    session.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) { Console.Write(d.Data.DeltaContent); sb.Append(d.Data.DeltaContent); }
        if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
        if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); idleTcs.TrySetResult(false); }
    });

    Console.Write("  IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
    Console.WriteLine("\n");
}

await client.StopAsync();
await client.DisposeAsync();
