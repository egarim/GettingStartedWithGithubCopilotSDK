// SendAndWaitAsync (bloquea hasta idle)
await using var session = await client.CreateSessionAsync();
var events = new List<string>();
session.On(evt => events.Add(evt.Type));

var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
Console.WriteLine($"  Respuesta: {response?.Data.Content}");
Console.WriteLine($"  Eventos tras retornar: {string.Join(", ", events.Distinct())}");
Console.WriteLine("  (session.idle ya esta en events porque SendAndWaitAsync bloquea)");