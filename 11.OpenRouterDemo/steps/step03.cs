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

        var (models, chosenModel) = await Step1_StartAndAuth(client);
        await Step2_ListModels(models);
        await Step3_ChatDefault(client);

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Paso 1: Iniciar cliente ────────────────────────────────────────
    async Task<(IReadOnlyList<ModelInfo> models, string chosenModel)> Step1_StartAndAuth(CopilotClient client)
    {
        PrintStep(1, Step1Text);
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        var auth = await client.GetAuthStatusAsync();
        PrintProp("Auth:", $"{auth.IsAuthenticated} ({auth.AuthType})");
        PrintProp("Fuente:", string.IsNullOrWhiteSpace(token) ? "VS Code login" : "GitHub PAT");
        Console.WriteLine();

        var models = await client.ListModelsAsync();

        var preferredModels = new[] { "claude-sonnet-4", "gpt-4.1", "gpt-4o" };
        string? chosenModel = null;
        foreach (var preferred in preferredModels)
        {
            if (models.Any(m => m.Id.Equals(preferred, StringComparison.OrdinalIgnoreCase)))
            {
                chosenModel = preferred;
                break;
            }
        }
        chosenModel ??= models.FirstOrDefault()?.Id ?? "gpt-4o";

        return (models, chosenModel);
    }

    // ── Paso 2: Listar modelos ─────────────────────────────────────────
    Task Step2_ListModels(IReadOnlyList<ModelInfo> models)
    {
        PrintStep(2, Step2Text);
        PrintProp("Total modelos:", models.Count);
        Console.WriteLine($"  {"ID",-45} {"Nombre",-30}");
        Console.WriteLine($"  {"--",-45} {"------",-30}");
        foreach (var m in models)
            Console.WriteLine($"  {m.Id,-45} {m.Name,-30}");
        Console.WriteLine();
        Console.WriteLine("  Los modelos custom de BYOK aparecen en esta lista!");
        Console.WriteLine();
        return Task.CompletedTask;
    }

    // ── Paso 3: Chat con modelo por defecto ────────────────────────────
    async Task Step3_ChatDefault(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        await using var session = await client.CreateSessionAsync();
        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
        Console.WriteLine($"  P: What model are you?");
        Console.WriteLine($"  R: {answer?.Data.Content}");
        Console.WriteLine();
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
