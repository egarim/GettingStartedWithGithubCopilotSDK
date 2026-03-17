// Helpers
CopilotClient CreateClient() => new(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});

static void PrintTitle(string title)
{
    Console.WriteLine("================================================================");
    Console.WriteLine($"  {title}");
    Console.WriteLine("================================================================\n");
}

static void PrintStep(int n, string text)
    => Console.WriteLine($"=== {n}. {text} ===");

static void PrintProp(string label, object? value)
    => Console.WriteLine($"  {label,-22} {value}");

static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();

[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"    [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 65%";
}

static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"    [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 65%";
}

static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"    [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
}