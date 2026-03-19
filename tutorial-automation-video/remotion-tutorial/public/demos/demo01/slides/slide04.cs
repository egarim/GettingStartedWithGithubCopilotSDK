// Paso 1: Crear el cliente
PrintStep(1, "Creando CopilotClient (UseLoggedInUser = true)");
var client = CreateClient();
PrintProp("Estado inicial:", client.State);
Console.WriteLine();