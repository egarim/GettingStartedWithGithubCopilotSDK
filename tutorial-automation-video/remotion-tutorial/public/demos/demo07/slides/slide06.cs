// ...
Console.WriteLine("  Enviando mensaje 2/3: Continuar con el castillo...");
var a2 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Continue the story with more details about the dragon's castle. Make it very long and descriptive."
});
Console.WriteLine($"  Respuesta 2 longitud: {a2?.Data.Content?.Length ?? 0} chars");
PrintProp("Eventos compactacion:", $"inicio={compactionStartEvents.Count}, completo={compactionCompleteEvents.Count}");