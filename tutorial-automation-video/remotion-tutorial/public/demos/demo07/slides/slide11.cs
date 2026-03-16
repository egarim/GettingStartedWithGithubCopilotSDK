// Compactacion desactivada
var compactionEvents = new List<SessionEvent>();

var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = false
    }
});