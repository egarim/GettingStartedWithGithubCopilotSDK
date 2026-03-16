// ...
Console.WriteLine("  Prompt: What is 7+7?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 7+7?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  Configuracion combinada aceptada");
await session.DisposeAsync();