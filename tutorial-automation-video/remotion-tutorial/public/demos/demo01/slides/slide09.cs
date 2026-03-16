// Listar modelos
if (!isAuth) { Console.WriteLine($"  {Strings.SkippedNoAuth}"); Console.WriteLine(); return; }

var models = await client.ListModelsAsync();
Console.WriteLine($"  Se encontraron {models.Count} modelo(s):");
Console.WriteLine($"  {"ID",-35} {"Name",-25} {"Capabilities"}");
Console.WriteLine($"  {"--",-35} {"----",-25} {"------------"}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-35} {m.Name,-25} {m.Capabilities?.ToString() ?? ""}");