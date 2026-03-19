#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 8: Handler de permisos al reanudar sesion
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

// Crear sesion original
var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What is 1+1?" });

// Reanudar con handler de permisos
var permissionReceived = false;
var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        permissionReceived = true;
        Console.WriteLine($"  [Permission on Resume] Kind: {request.Kind} -> APROBADO");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});

await session2.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo resumed-with-permissions'"
});
Console.WriteLine($"Handler disparado al reanudar: {permissionReceived}");

await session2.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
