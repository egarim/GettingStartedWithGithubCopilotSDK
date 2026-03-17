// ...
Console.WriteLine("  Prompt: What is 2+2?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  Sesion funciona con configuracion MCP");
await session.DisposeAsync();