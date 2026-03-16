#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 6: ToolCallId en solicitudes de permisos
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var toolCallIds = new List<string>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        if (!string.IsNullOrEmpty(request.ToolCallId))
        {
            toolCallIds.Add(request.ToolCallId);
            Console.WriteLine($"  [Permission] ToolCallId: {request.ToolCallId}");
        }
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});

await session.SendAndWaitAsync(new MessageOptions { Prompt = "Run 'echo test-toolcallid'" });
Console.WriteLine($"ToolCallIds recibidos: {toolCallIds.Count}");
// -> Cada solicitud tiene un ToolCallId unico para correlacionar

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
