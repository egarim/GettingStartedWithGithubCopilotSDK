#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Handler asincrono de permisos
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = async (request, invocation) =>
    {
        Console.WriteLine($"  [Permission] Kind: {request.Kind} - Verificando...");
        await Task.Delay(500); // simular verificacion asincrona (DB, API, etc.)
        Console.WriteLine("  [Permission] Aprobado tras espera");
        return new PermissionRequestResult { Kind = "approved" };
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo hello from async permission demo' and tell me the output"
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
