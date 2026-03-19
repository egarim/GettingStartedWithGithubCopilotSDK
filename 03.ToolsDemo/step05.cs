#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Manejo de errores en herramientas
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

// Herramienta que siempre lanza excepcion
var failingTool = AIFunctionFactory.Create(
    () => { throw new Exception("Secret Internal Error - Melbourne"); },
    "get_user_location", "Gets the user's location");

var session = await client.CreateSessionAsync(new SessionConfig { Tools = [failingTool] });

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "What is my location? If you can't find out, just say 'unknown'."
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
Console.WriteLine($"Contiene 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
// -> False (el SDK NO expone detalles de excepciones al modelo)

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
