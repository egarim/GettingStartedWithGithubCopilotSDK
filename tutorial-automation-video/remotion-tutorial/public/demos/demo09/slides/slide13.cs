// ...
PrintProp("Agente:", "devops-agent");
PrintProp("Herramientas:", "[\"bash\", \"edit\"] <- conjunto restringido");

var session = await client.CreateSessionAsync(new SessionConfig
{
    CustomAgents = customAgents
});
PrintProp("Sesion:", $"{session.SessionId}");
await session.DisposeAsync();