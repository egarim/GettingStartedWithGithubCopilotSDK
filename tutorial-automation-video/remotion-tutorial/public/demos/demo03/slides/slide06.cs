// Paso 5: Manejo de errores en herramientas
// Herramienta que siempre lanza excepcion
var failingTool = AIFunctionFactory.Create(
    () => { throw new Exception("Secret Internal Error - Melbourne"); },
    "get_user_location", "Gets the user's location");
var session = await client.CreateSessionAsync(new SessionConfig { Tools = [failingTool] });
    Prompt = "What is my location? If you can't find out, just say 'unknown'."
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
Console.WriteLine($"Contiene 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
// -> False (el SDK NO expone detalles de excepciones al modelo)