// Paso 3: Multiples herramientas personalizadas
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
    new MessageOptions { Prompt = "What's the weather in Madrid and what time is it there?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> usa ambas herramientas
[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
    => $"Weather in {city}: 22C, partly cloudy, humidity 65%";
[Description("Gets the current time for a city/timezone")]
static string GetTime([Description("City name")] string city)
    => $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";