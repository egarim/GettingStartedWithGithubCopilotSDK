// ...
var testFile = Path.Combine(workDir, "test.txt");
await File.WriteAllTextAsync(testFile, "original content");

Console.WriteLine($"  Prompt: Edit the file at {testFile} and replace 'original' with 'modified'");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
PrintProp("Solicitudes recibidas:", permissionRequests.Count);
Console.WriteLine($"  Permisos de escritura: {permissionRequests.Count(r => r.Kind == "write")}");