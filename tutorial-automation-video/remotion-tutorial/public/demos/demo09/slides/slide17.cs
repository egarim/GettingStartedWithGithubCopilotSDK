// ...
var customAgents = new List<CustomAgentConfig>
{
    new CustomAgentConfig
    {
        Name = "coordinator-agent",
        DisplayName = "Coordinator Agent",
        Description = "Coordinates tasks across MCP servers and other agents",
        Prompt = "You are a coordinator that can access shared MCP servers."
    }
};