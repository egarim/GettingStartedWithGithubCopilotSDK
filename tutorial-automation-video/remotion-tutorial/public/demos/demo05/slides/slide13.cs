// ...
Console.WriteLine("  Prompt: Run 'echo test-toolcallid'");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo test-toolcallid'"
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(150, answer?.Data.Content?.Length ?? 0))}");
PrintProp("ToolCallIds recibidos:", toolCallIds.Count);
foreach (var id in toolCallIds) Console.WriteLine($"    -> {id}");
await session.DisposeAsync();