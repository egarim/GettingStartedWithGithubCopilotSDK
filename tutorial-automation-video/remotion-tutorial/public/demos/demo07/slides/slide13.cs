// ...
Console.WriteLine("  Prompt: What is 2+2?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
PrintProp("Eventos disparados:", compactionEvents.Count);
Console.WriteLine("  (Esperado: 0 - compactacion desactivada)");