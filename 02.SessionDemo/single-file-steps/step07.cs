#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 7: Reanudar sesion con ResumeSessionAsync
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
await session1.SendAndWaitAsync(new MessageOptions { Prompt = "Remember this number: 42" });

var session2 = await client.ResumeSessionAsync(sessionId);
Console.WriteLine($"ID coincide: {session2.SessionId == sessionId}"); // -> True

var a2 = await session2.SendAndWaitAsync(
    new MessageOptions { Prompt = "What number did I ask you to remember?" });
Console.WriteLine($"Respuesta: {a2?.Data.Content}"); // -> 42

await session2.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
