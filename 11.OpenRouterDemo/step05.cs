#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Comparar multiples modelos con el mismo prompt
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

var models = await client.ListModelsAsync();
var modelsToTry = models.Select(m => m.Id).Take(4).ToList();
const string testPrompt = "What is the capital of Australia? One sentence.";

foreach (var modelId in modelsToTry)
{
    try
    {
        await using var session = await client.CreateSessionAsync(new SessionConfig { Model = modelId });
        var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = testPrompt });
        Console.WriteLine($"  {modelId,-45} -> {answer?.Data.Content?.Trim()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {modelId,-45} -> Error: {ex.Message.Split('\n')[0]}");
    }
}

await client.StopAsync();
await client.DisposeAsync();
