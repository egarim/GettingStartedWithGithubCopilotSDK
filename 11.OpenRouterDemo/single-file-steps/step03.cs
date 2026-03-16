#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Chat con el modelo por defecto
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

await using var session = await client.CreateSessionAsync();  // modelo por defecto
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
Console.WriteLine($"  R: {answer?.Data.Content}");

await client.StopAsync();
await client.DisposeAsync();
