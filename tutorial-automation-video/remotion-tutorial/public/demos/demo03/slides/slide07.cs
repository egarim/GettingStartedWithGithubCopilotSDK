// Paso 6: Filtros AvailableTools y ExcludedTools
// AvailableTools: solo permitir herramientas especificas
var s1 = await client.CreateSessionAsync(new SessionConfig
    AvailableTools = new List<string> { "view", "edit" }
var a1 = await s1.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have?" });
Console.WriteLine($"AvailableTools: {a1?.Data.Content?.Substring(0, Math.Min(150, a1.Data.Content?.Length ?? 0))}");
await s1.DisposeAsync();
// ExcludedTools: excluir herramientas especificas
var s2 = await client.CreateSessionAsync(new SessionConfig
    ExcludedTools = new List<string> { "view" }
var a2 = await s2.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have?" });
Console.WriteLine($"ExcludedTools: {a2?.Data.Content?.Substring(0, Math.Min(150, a2.Data.Content?.Length ?? 0))}");
await s2.DisposeAsync();