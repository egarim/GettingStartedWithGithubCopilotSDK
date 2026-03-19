#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.AI@*
#:package Microsoft.Extensions.Logging.Console@*

// Paso 6: Modelo custom + herramientas (tools)
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();

var chosenModel = "gpt-4o";

[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
    => $"Weather in {city}: 22C, partly cloudy, humidity 55%";

[Description("Gets the current time for a city")]
static string GetTime([Description("City name")] string city)
    => $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel,
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Tokyo and what time is it there?" });
Console.WriteLine($"  R: {answer?.Data.Content}");

await client.StopAsync();
await client.DisposeAsync();
