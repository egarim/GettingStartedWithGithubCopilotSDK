// Paso 4: Chat con un modelo especifico (BYOK o built-in)
var chosenModel = "gpt-4o";  // o cualquier modelo de ListModelsAsync()
await using var session = await client.CreateSessionAsync(new SessionConfig
    Model = chosenModel  // seleccionar modelo especifico
Console.WriteLine($"  Modelo: {chosenModel}");