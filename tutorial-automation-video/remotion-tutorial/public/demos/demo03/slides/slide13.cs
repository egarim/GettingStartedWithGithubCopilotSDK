// ...
Console.WriteLine("  ExcludedTools = [\"view\"] -> 'view' excluido, los demas permanecen");
var session2 = await client.CreateSessionAsync(new SessionConfig
{
    ExcludedTools = new List<string> { "view" }
});
Console.WriteLine("  Prompt: What tools do you have available?");
var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
Console.WriteLine($"  Respuesta: {a2?.Data.Content?.Substring(0, Math.Min(200, a2.Data.Content?.Length ?? 0))}");
await session2.DisposeAsync();