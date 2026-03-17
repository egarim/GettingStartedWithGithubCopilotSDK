// ...
var protectedFile = Path.Combine(workDir, "protected.txt");
await File.WriteAllTextAsync(protectedFile, "protected content");

Console.WriteLine($"  Prompt: Edit the file at {protectedFile} and replace 'protected' with 'hacked'");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {protectedFile} and replace 'protected' with 'hacked'"
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");