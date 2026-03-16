// Filtros AvailableTools y ExcludedTools

Console.WriteLine("  AvailableTools = [\"view\", \"edit\"] -> solo estas 2 herramientas built-in");
var session1 = await client.CreateSessionAsync(new SessionConfig
{
    AvailableTools = new List<string> { "view", "edit" }
});
Console.WriteLine("  Prompt: What tools do you have available?");
var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
Console.WriteLine($"  Respuesta: {a1?.Data.Content?.Substring(0, Math.Min(200, a1.Data.Content?.Length ?? 0))}");
await session1.DisposeAsync();