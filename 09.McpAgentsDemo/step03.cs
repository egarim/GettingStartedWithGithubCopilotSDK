#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Multiples servidores MCP en una sesion
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

var session = await client.CreateSessionAsync(new SessionConfig
{
    McpServers = new Dictionary<string, object>
    {
        ["filesystem-server"] = new McpLocalServerConfig
        {
            Type = "local", Command = "echo", Args = ["filesystem"], Tools = ["*"]
        },
        ["database-server"] = new McpLocalServerConfig
        {
            Type = "local", Command = "echo", Args = ["database"], Tools = ["*"]
        }
    }
});
Console.WriteLine($"  Sesion con 2 servidores MCP: {session.SessionId}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
