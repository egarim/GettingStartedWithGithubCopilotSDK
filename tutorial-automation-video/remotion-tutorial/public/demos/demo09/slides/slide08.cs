// Multiples servidores MCP
var mcpServers = new Dictionary<string, object>
{
    ["filesystem-server"] = new McpLocalServerConfig
    {
        Type = "local",
        Command = "echo",
        Args = ["filesystem-server"],
        Tools = ["*"]
    },
    ["database-server"] = new McpLocalServerConfig
    {
        Type = "local",
        Command = "echo",
        Args = ["database-server"],
// ...