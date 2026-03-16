// Suscripcion a eventos
await using var session = await client.CreateSessionAsync();
var receivedEvents = new List<string>();
var idleTcs = new TaskCompletionSource<bool>();

var sub = session.On(evt =>
{
    receivedEvents.Add(evt.GetType().Name);
    if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
});