// ...
var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
{
    McpServers = new Dictionary<string, object>
    {
        ["resume-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["hello-resume"],
            Tools = ["*"]
        }
    },
    CustomAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "resume-agent",
            DisplayName = "Resume Agent",
            Description = "Added on resume",
            Prompt = "You are a resume agent."
        }
    }
});

PrintProp("Sesion reanudada:", session2.SessionId);
Console.WriteLine("  Prompt: What is 3+3?");
var answer = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  MCP y agentes agregados exitosamente al reanudar");
await session2.DisposeAsync();