#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Chat con un modelo especifico (BYOK o built-in)
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();

var chosenModel = "gpt-4o";  // o cualquier modelo de ListModelsAsync()
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel  // seleccionar modelo especifico
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
Console.WriteLine($"  Modelo: {chosenModel}");
Console.WriteLine($"  R: {answer?.Data.Content}");

await client.StopAsync();
await client.DisposeAsync();
