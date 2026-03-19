// Paso 2: Compactacion activada - Umbrales bajos
var session = await client.CreateSessionAsync(new SessionConfig
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.005,  // umbral bajo para demo
        BufferExhaustionThreshold = 0.01
    }
session.On(evt =>
    if (evt is SessionCompactionStartEvent)
        Console.WriteLine("  * Compactacion de fondo activada!");
    if (evt is SessionCompactionCompleteEvent c)
        Console.WriteLine($"  OK Exito: {c.Data.Success}, Tokens removidos: {c.Data.TokensRemoved}");
var a1 = await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Tell me a long story about a dragon. Be very detailed."
Console.WriteLine($"  Respuesta 1: {a1?.Data.Content?.Length ?? 0} chars");
await session.DisposeAsync();