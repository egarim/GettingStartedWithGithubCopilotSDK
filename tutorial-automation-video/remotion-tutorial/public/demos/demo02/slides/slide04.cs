// Paso 3: Conversacion multi-turno con SendAndWaitAsync
await using var session = await client.CreateSessionAsync();
var a1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
Console.WriteLine($"A1: {a1?.Data.Content}"); // -> 25
var a2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
Console.WriteLine($"A2: {a2?.Data.Content}"); // -> 50 (recuerda el contexto)