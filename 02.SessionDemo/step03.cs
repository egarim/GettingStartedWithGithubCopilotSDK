#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Conversacion multi-turno con SendAndWaitAsync
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

await using var session = await client.CreateSessionAsync();

var a1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
Console.WriteLine($"A1: {a1?.Data.Content}"); // -> 25

var a2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
Console.WriteLine($"A2: {a2?.Data.Content}"); // -> 50 (recuerda el contexto)

await client.StopAsync();
await client.DisposeAsync();
