// Paso 3: Denegar permiso - Bloquear modificaciones
        Console.WriteLine($"  [Permission] Kind: {request.Kind} -> DENEGADO");
        return Task.FromResult(new PermissionRequestResult
        {
            Kind = "denied-interactively-by-user"
        });
var protectedFile = Path.Combine(workDir, "protected.txt");
await File.WriteAllTextAsync(protectedFile, "protected content");
await session.SendAndWaitAsync(new MessageOptions
    Prompt = $"Edit the file at {protectedFile} and replace 'protected' with 'hacked'"
var content = await File.ReadAllTextAsync(protectedFile);
Console.WriteLine($"Contenido intacto: {content == "protected content"}"); // -> True