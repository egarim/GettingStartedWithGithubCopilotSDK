#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Multiples herramientas personalizadas
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Madrid and what time is it there?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> usa ambas herramientas

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
    => $"Weather in {city}: 22C, partly cloudy, humidity 65%";

[Description("Gets the current time for a city/timezone")]
static string GetTime([Description("City name")] string city)
    => $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
