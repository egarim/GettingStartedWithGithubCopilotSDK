// ...
Console.WriteLine("  Verificando que la sesion funciona tras compactacion...");
Console.WriteLine("  Prompt: What was the main story about? Answer in one sentence.");
var a4 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "What was the main story about? Answer in one sentence."
});
Console.WriteLine($"  Respuesta: {a4?.Data.Content}");
Console.WriteLine("  (Deberia recordar que era sobre un dragon - contexto preservado via resumen)");