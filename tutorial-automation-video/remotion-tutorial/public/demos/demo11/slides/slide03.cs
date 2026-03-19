// Paso 2: Listar todos los modelos (built-in + BYOK)
    UseLoggedInUser = true,
var models = await client.ListModelsAsync();
Console.WriteLine($"  Total modelos: {models.Count}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-45} {m.Name}");
// Los modelos custom de BYOK aparecen en esta lista!