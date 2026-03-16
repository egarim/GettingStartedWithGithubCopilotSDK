// ...
Console.WriteLine("  -- Resultados de compactacion --");
PrintProp("CompactionStart:", compactionStartEvents.Count);
PrintProp("CompactionComplete:", compactionCompleteEvents.Count);

if (compactionCompleteEvents.Count > 0)
{
    var last = compactionCompleteEvents[^1];
    PrintProp("Ultima exitosa:", last.Data.Success);
    PrintProp("Tokens removidos:", last.Data.TokensRemoved);
}
else
{
    Console.WriteLine("  (No se activo compactacion - la ventana de contexto puede no haberse llenado suficiente)");
}