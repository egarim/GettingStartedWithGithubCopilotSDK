// Paso 7: Parada ordenada
PrintStep(7, "Parada ordenada (StopAsync)");
await client.StopAsync();
PrintProp("Estado tras parar:", client.State);
Console.WriteLine("Demo completado!");