// ...
var content = await File.ReadAllTextAsync(testFile);
Console.WriteLine($"  Contenido despues: \"{content}\"");
await session.DisposeAsync();