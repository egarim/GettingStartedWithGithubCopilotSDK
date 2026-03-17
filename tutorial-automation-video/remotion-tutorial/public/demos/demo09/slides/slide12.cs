// ...
Console.WriteLine("  Prompt: What is 5+5?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 5+5?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  Configuracion de agente aceptada");
await session.DisposeAsync();