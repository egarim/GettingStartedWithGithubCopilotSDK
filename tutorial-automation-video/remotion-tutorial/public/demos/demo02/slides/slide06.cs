// Paso 5: SendAsync (disparar y olvidar) - retorna ANTES de idle
var events = new List<string>();
session.On(evt => events.Add(evt.Type));
await session.SendAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"idle en events? {events.Contains("session.idle")}"); // -> False
// Hay que esperar manualmente a que llegue session.idle
var tcs = new TaskCompletionSource<bool>();
session.On(evt => { if (evt is SessionIdleEvent) tcs.TrySetResult(true); });
await tcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
Console.WriteLine($"idle en events? {events.Contains("session.idle")}"); // -> True