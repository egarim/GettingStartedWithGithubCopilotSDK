// Comportamiento por defecto
var session = await client.CreateSessionAsync(new SessionConfig());

Console.WriteLine("  Prompt: What is 2+2?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  (Funciona sin handler - solo se activa para operaciones de escritura/ejecucion)");
await session.DisposeAsync();