#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 8: Combinacion - Servidores MCP + Agentes en la misma sesion
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
        ["shared-server"] = new McpLocalServerConfig
        {
            Type = "local", Command = "echo", Args = ["shared"], Tools = ["*"]
        }
    },
    CustomAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "coordinator-agent",
            DisplayName = "Coordinator Agent",
            Description = "Coordinates tasks across MCP servers",
            Prompt = "You are a coordinator that can access shared MCP servers."
        }
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 7+7?" });
Console.WriteLine($"  Respuesta (MCP + agente): {answer?.Data.Content}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
