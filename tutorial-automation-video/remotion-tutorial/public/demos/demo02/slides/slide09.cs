// SendAsync (disparar y olvidar)
await using var session = await client.CreateSessionAsync();
var events = new List<string>();
session.On(evt => events.Add(evt.Type));

Console.WriteLine("  Prompt: What is 2+2?");
await session.SendAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Despues de SendAsync -> session.idle en events? {events.Contains("session.idle")}");
Console.WriteLine("  (Esperado: False - SendAsync retorna antes de que termine el turno)");

var finalMsg = await WaitForIdleAsync(session);
Console.WriteLine($"  Despues de esperar -> session.idle en events? {events.Contains("session.idle")}");
Console.WriteLine($"  Respuesta: {finalMsg?.Data.Content}");