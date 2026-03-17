// Helpers
CopilotClient CreateClient()
{
    var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    return new CopilotClient(new CopilotClientOptions
    {
        GithubToken = string.IsNullOrWhiteSpace(token) ? null : token,
        UseLoggedInUser = string.IsNullOrWhiteSpace(token),
        Logger = logger
    });
}

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

static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 55%";
}

static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";
}