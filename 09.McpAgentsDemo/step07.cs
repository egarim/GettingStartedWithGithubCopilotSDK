#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 7: Multiples agentes personalizados
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
            Name = "frontend-agent",
            DisplayName = "Frontend Agent",
            Description = "Specializes in React, CSS, and UI",
            Prompt = "You are a frontend development expert."
        },
        new CustomAgentConfig
        {
            Name = "backend-agent",
            DisplayName = "Backend Agent",
            Description = "Specializes in C#, .NET, and APIs",
            Prompt = "You are a backend development expert.",
            Infer = false  // solo se invoca explicitamente
        }
    }
});
Console.WriteLine("  2 agentes configurados: frontend-agent, backend-agent");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
