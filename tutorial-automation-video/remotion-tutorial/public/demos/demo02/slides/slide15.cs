// Deltas en streaming
await using var session = await client.CreateSessionAsync(new SessionConfig { Streaming = true });
var buffer = new StringBuilder();
var idleTcs = new TaskCompletionSource<bool>();

session.On(evt =>
{
    switch (evt)
    {
        case AssistantMessageDeltaEvent delta:
            Console.Write(delta.Data.DeltaContent);
            buffer.Append(delta.Data.DeltaContent);
            break;
        case SessionIdleEvent:
            idleTcs.TrySetResult(true);
// ...