// Listar modelos
PrintProp("Total modelos:", models.Count);
Console.WriteLine($"  {"ID",-45} {"Nombre",-30}");
Console.WriteLine($"  {"--",-45} {"------",-30}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-45} {m.Name,-30}");
Console.WriteLine("  Los modelos custom de BYOK aparecen en esta lista!");
return Task.CompletedTask;