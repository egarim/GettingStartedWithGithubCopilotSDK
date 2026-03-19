// Paso 11: Deltas en streaming (AssistantMessageDeltaEvent)
await using var session = await client.CreateSessionAsync(new SessionConfig { Streaming = true });
var buffer = new StringBuilder();
var idleTcs = new TaskCompletionSource<bool>();
session.On(evt =>
    switch (evt)
        case AssistantMessageDeltaEvent delta:
            Console.Write(delta.Data.DeltaContent); // imprime token por token
            buffer.Append(delta.Data.DeltaContent);
            break;
        case SessionIdleEvent:
            idleTcs.TrySetResult(true);
            break;
await session.SendAsync(new MessageOptions { Prompt = "Tell me a very short joke." });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
Console.WriteLine($"\nCaracteres transmitidos: {buffer.Length}");