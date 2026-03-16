// Combinacion MCP + Agentes
var mcpServers = new Dictionary<string, object>
{
    ["shared-server"] = new McpLocalServerConfig
    {
        Type = "local",
        Command = "echo",
        Args = ["shared"],
        Tools = ["*"]
    }
};