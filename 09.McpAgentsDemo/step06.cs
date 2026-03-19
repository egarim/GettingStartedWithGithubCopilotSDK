#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 6: Agente con sus propios servidores MCP
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
    CustomAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "data-agent",
            DisplayName = "Data Agent",
            Description = "An agent with its own MCP server",
            Prompt = "You are a data agent with database access.",
            McpServers = new Dictionary<string, object>  // MCP aislado por agente
            {
                ["agent-db-server"] = new McpLocalServerConfig
                {
                    Type = "local", Command = "echo", Args = ["agent-data"], Tools = ["*"]
                }
            }
        }
    }
});
Console.WriteLine("  Agente con MCP propio aislado");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
