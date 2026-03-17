// ...
Console.WriteLine("  Enviando mensaje 1/3: Historia larga de un dragon...");
var a1 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Tell me a long story about a dragon. Be very detailed. Include at least 5 paragraphs."
});
Console.WriteLine($"  Respuesta 1 longitud: {a1?.Data.Content?.Length ?? 0} chars");
PrintProp("Eventos compactacion:", $"inicio={compactionStartEvents.Count}, completo={compactionCompleteEvents.Count}");