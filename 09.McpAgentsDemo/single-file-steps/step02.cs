#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 2: Configuracion de un servidor MCP
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
        ["test-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["hello-mcp"],
            Tools = ["*"]  // todas las herramientas del servidor
        }
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}"); // Sesion funciona con MCP

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
