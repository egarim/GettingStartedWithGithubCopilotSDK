// Paso 6: Listar modelos disponibles
PrintStep(6, "Listar modelos disponibles (ListModelsAsync)");
var models = await client.ListModelsAsync();
Console.WriteLine($"  Se encontraron {models.Count} modelo(s):");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-35} {m.Name}");