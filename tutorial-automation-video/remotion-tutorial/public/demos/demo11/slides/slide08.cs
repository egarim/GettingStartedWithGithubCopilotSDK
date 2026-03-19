// Paso 7: Streaming con modelo custom
    Streaming = true  // habilitar streaming con modelo BYOK
var sb = new StringBuilder();
var idleTcs = new TaskCompletionSource<bool>();
session.On(evt =>
    if (evt is AssistantMessageDeltaEvent delta)
    {
        Console.Write(delta.Data.DeltaContent);
        sb.Append(delta.Data.DeltaContent);
    }
    if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
Console.Write("  Streaming: ");
await session.SendAsync(
    new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
Console.WriteLine($"\n  Total chars: {sb.Length}");