// ...
var session = await client.CreateSessionAsync(new SessionConfig
{
    McpServers = mcpServers
});
PrintProp("Sesion creada:", session.SessionId);