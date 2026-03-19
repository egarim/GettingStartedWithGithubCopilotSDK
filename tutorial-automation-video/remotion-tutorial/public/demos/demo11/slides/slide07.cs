// Paso 6: Modelo custom + herramientas (tools)
var chosenModel = "gpt-4o";
[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
    => $"Weather in {city}: 22C, partly cloudy, humidity 55%";
[Description("Gets the current time for a city")]
static string GetTime([Description("City name")] string city)
    => $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";
await using var session = await client.CreateSessionAsync(new SessionConfig
    Model = chosenModel,
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Tokyo and what time is it there?" });
Console.WriteLine($"  R: {answer?.Data.Content}");