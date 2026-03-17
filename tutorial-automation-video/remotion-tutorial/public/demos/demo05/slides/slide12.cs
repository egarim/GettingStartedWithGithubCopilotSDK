// ...
Console.WriteLine("  Prompt: Run 'echo hello from async permission demo' and tell me the output");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo hello from async permission demo' and tell me the output"
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
await session.DisposeAsync();