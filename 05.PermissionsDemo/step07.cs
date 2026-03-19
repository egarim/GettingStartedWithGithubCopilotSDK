#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 7: Error en handler - Degradacion elegante
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine("  [Permission] Lanzando excepcion!");
        throw new InvalidOperationException("Simulated handler crash");
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo test'. If you can't, say 'failed'."
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> El SDK maneja la excepcion: permiso denegado automaticamente

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
