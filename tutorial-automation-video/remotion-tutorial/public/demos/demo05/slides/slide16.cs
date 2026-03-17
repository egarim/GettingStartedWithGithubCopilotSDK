// ...
Console.WriteLine("  Prompt: Run 'echo test'. If you can't, say 'failed'.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo test'. If you can't, say 'failed'."
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
Console.WriteLine("  (El SDK maneja la excepcion elegantemente - permiso denegado automaticamente)");
await session.DisposeAsync();