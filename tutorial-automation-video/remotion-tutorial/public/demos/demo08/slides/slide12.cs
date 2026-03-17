// Sin skill (linea base)
var session = await client.CreateSessionAsync(new SessionConfig());

Console.WriteLine("  Prompt: Say hello briefly.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly."
});
Console.WriteLine($"  Respuesta (sin skill): {answer?.Data.Content}");
PrintProp("Contiene marcador:", answer?.Data.Content?.Contains(SkillMarker) ?? false);
Console.WriteLine("  (Esperado: Sin marcador - sin skill cargado)");

await session.DisposeAsync();