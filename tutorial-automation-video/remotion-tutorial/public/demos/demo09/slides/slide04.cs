// Servidor MCP simple
var mcpServers = new Dictionary<string, object>
{
    ["test-server"] = new McpLocalServerConfig
    {
        Type = "local",
        Command = "echo",
        Args = ["hello-mcp"],
        Tools = ["*"]
    }
};