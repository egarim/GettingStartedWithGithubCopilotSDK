// Paso 2: Iniciar el cliente
PrintStep(2, "Iniciando cliente (StartAsync)");
await client.StartAsync();
PrintProp("Estado tras iniciar:", client.State);