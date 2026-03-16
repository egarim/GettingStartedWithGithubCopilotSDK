// ...
session.On(evt =>
{
    if (evt is SessionCompactionStartEvent startEvt)
    {
        compactionStartEvents.Add(startEvt);
        Console.WriteLine("    * [CompactacionInicio] Compactacion de fondo activada!");
    }
    if (evt is SessionCompactionCompleteEvent completeEvt)
    {
        compactionCompleteEvents.Add(completeEvt);
        Console.WriteLine($"    OK [CompactacionCompleta] Exito: {completeEvt.Data.Success}, Tokens removidos: {completeEvt.Data.TokensRemoved}");
    }
});