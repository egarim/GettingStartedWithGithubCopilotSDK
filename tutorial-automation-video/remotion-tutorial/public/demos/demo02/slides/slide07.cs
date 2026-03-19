// Paso 6: SendAndWaitAsync (bloquea hasta idle)
var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
Console.WriteLine($"Respuesta: {response?.Data.Content}"); // -> 6
Console.WriteLine($"Eventos: {string.Join(", ", events.Distinct())}");
// -> session.idle YA esta en events (SendAndWaitAsync bloquea hasta idle)