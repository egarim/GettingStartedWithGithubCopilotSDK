// ...
Console.WriteLine("  McpServers: { \"shared-server\": { ... } }");
Console.WriteLine("  CustomAgents: [ { name: \"coordinator-agent\", ... } ]");
Console.WriteLine("  (Tanto servidores MCP como agentes en la misma sesion)\n");

var session = await client.CreateSessionAsync(new SessionConfig
{
    McpServers = mcpServers,
    CustomAgents = customAgents
});