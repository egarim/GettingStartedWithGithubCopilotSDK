// Compactacion activada
Console.WriteLine("  (Umbrales bajos activan compactacion rapido para demostracion)\n");

var compactionStartEvents = new List<SessionCompactionStartEvent>();
var compactionCompleteEvents = new List<SessionCompactionCompleteEvent>();

var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.005,
        BufferExhaustionThreshold = 0.01
    }
});