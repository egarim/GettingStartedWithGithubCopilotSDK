// ...
var content = await File.ReadAllTextAsync(protectedFile);
Console.WriteLine($"  Contenido (debe estar intacto): \"{content}\"");
Console.WriteLine($"  Archivo protegido: {content == "protected content"}");
await session.DisposeAsync();