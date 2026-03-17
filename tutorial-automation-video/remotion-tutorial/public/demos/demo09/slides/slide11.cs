// ...
var session = await client.CreateSessionAsync(new SessionConfig
{
    CustomAgents = customAgents
});
PrintProp("Sesion:", session.SessionId);