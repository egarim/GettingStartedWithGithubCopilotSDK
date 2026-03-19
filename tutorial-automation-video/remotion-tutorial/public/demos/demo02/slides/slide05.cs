// Paso 4: Suscripcion a eventos con session.On
var receivedEvents = new List<string>();
var idleTcs = new TaskCompletionSource<bool>();
var sub = session.On(evt =>
{
    receivedEvents.Add(evt.GetType().Name);
    if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
});
await session.SendAsync(new MessageOptions { Prompt = "What is 100 + 200?" });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
sub.Dispose();
foreach (var e in receivedEvents)
    Console.WriteLine($"  {e}"); // -> AssistantMessageEvent, SessionIdleEvent, etc.