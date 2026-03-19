// Paso 2: Aprobar permiso - Operaciones de escritura
await client.StartAsync();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine($"  [Permission] Kind: {request.Kind} -> APROBADO");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});
var testFile = Path.Combine(workDir, "test.txt");
await File.WriteAllTextAsync(testFile, "original content");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
});
Console.WriteLine($"Contenido despues: \"{await File.ReadAllTextAsync(testFile)}\"");
// -> "modified content"
await session.DisposeAsync();
await client.StopAsync();