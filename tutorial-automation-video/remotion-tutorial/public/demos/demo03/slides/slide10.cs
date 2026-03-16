// ...
Console.WriteLine("  Prompt: What is my location? If you can't find out, just say 'unknown'.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "What is my location? If you can't find out, just say 'unknown'."
});
Console.WriteLine($"  La herramienta lanzo una excepcion con 'Melbourne' en el mensaje.");
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  Contiene 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
Console.WriteLine("  (Esperado: Falso - el SDK NO expone detalles de excepciones al modelo)");
await session.DisposeAsync();