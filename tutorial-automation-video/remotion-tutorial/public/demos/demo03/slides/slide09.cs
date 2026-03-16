// ...
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [failingTool]
});