// Agente con MCP propio
var customAgents = new List<CustomAgentConfig>
{
    new CustomAgentConfig
    {
        Name = "data-agent",
        DisplayName = "Data Agent",
        Description = "An agent with its own MCP server for data access",
        Prompt = "You are a data agent with access to a database MCP server.",
        McpServers = new Dictionary<string, object>
        {
            ["agent-db-server"] = new McpLocalServerConfig
            {
                Type = "local",
                Command = "echo",
// ...