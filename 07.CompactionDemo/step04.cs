#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Sesion infinita interactiva con streaming
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

var compactionCount = 0;
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.005,
        BufferExhaustionThreshold = 0.01
    }
});

session.On(evt =>
{
    if (evt is SessionCompactionStartEvent)
        Console.WriteLine("\n  * COMPACTACION INICIADA");
    if (evt is SessionCompactionCompleteEvent c)
    {
        compactionCount++;
        Console.WriteLine($"  OK COMPACTACION #{compactionCount} - removidos {c.Data.TokensRemoved} tokens");
    }
});

Console.WriteLine("Sesion infinita activa - sigue chateando (vacio para salir):\n");
while (true)
{
    Console.Write($"  Tu [{compactionCount} compactaciones]: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    session.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
    });

    Console.Write("  IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine("\n");
}

await client.StopAsync();
await client.DisposeAsync();
