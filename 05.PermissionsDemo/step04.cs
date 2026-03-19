#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Comportamiento por defecto (sin handler)
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

// Sin OnPermissionRequest - funciona para consultas sin escritura
var session = await client.CreateSessionAsync(new SessionConfig());

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> 4
// (Solo se activa para operaciones de escritura/ejecucion)

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
