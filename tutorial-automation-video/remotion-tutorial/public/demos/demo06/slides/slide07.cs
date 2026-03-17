// ...
Console.WriteLine("  Prompt: Ask me to choose between 'Option A' and 'Option B' using the ask_user tool.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Ask me to choose between 'Option A' and 'Option B' using the ask_user tool. Wait for my response before continuing."
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
PrintProp("Solicitudes recibidas:", userInputRequests.Count);
foreach (var req in userInputRequests)
{
    Console.WriteLine($"    Pregunta: {req.Question}");
    Console.WriteLine($"    Tiene opciones: {req.Choices is { Count: > 0 }}");
}
await session.DisposeAsync();