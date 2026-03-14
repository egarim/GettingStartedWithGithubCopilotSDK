using System.ComponentModel;
using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new BYOKDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class BYOKDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "11 - DEMO: Bring Your Own Key (BYOK) - Modelos personalizados";
    const string Step1Text        = "Iniciar cliente y verificar auth";
    const string Step2Text        = "Listar todos los modelos (built-in + BYOK)";
    const string Step3Text        = "Chat con el modelo por defecto";
    const string Step4Text        = "Chat con un modelo especifico";
    const string Step5Text        = "Comparar multiples modelos";
    const string Step6Text        = "Modelo custom + herramientas";
    const string Step7Text        = "Streaming con modelo custom";
    const string InteractiveHint  = "Presiona Enter para chat interactivo con el modelo BYOK.";
    const string InteractivePrompt = "Escribe mensajes (vacio para salir). Los modelos BYOK funcionan exactamente igual.\n";

    // ── Helpers ─────────────────────────────────────────────────────────
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

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var client = CreateClient();
        await client.StartAsync();

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Implementaciones de herramientas ────────────────────────────────
    [Description("Gets the current weather for a city")]
    static string GetWeather([Description("City name")] string city)
    {
        Console.WriteLine($"  [Tool:get_weather] city={city}");
        return $"Weather in {city}: 22°C, partly cloudy, humidity 55%";
    }

    [Description("Gets the current time for a city")]
    static string GetTime([Description("City name")] string city)
    {
        Console.WriteLine($"  [Tool:get_time] city={city}");
        return $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";
    }
}
