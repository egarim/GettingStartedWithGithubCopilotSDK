#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Agente con herramientas especificas
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
            Name = "devops-agent",
            DisplayName = "DevOps Agent",
            Description = "An agent for DevOps tasks",
            Prompt = "You are a DevOps agent. You can use bash and edit tools.",
            Tools = ["bash", "edit"],  // conjunto restringido de herramientas
            Infer = true
        }
    }
});
Console.WriteLine($"  Agente devops con Tools: [\"bash\", \"edit\"]");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
