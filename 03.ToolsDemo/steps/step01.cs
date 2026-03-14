using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new ToolsDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class ToolsDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle         = "03 - DEMO: Herramientas personalizadas (AIFunction)";
    const string Step1Text         = "Herramienta personalizada simple (encrypt_string)";
    const string Step2Text         = "Multiples herramientas personalizadas";
    const string Step3Text         = "Tipos complejos de entrada/salida (records, arrays)";
    const string Step4Text         = "Manejo de errores en herramientas";
    const string Step5Text         = "Filtros AvailableTools y ExcludedTools";
    const string InteractiveHint   = "Presiona Enter para modo interactivo con herramientas, o Ctrl+C para salir.";
    const string InteractiveTools  = "Herramientas: encrypt_string, get_weather, get_time";
    const string InteractivePrompt = "Escribe mensajes (vacio para salir):";

    // ── Helpers ─────────────────────────────────────────────────────────
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

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var client = CreateClient();
        await client.StartAsync();
        Console.WriteLine("  Cliente iniciado.\n");

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Implementaciones de herramientas ────────────────────────────────
    [Description("Encrypts a string by converting it to uppercase")]
    static string EncryptString([Description("String to encrypt")] string input)
        => input.ToUpperInvariant();

    [Description("Gets the current weather for a city")]
    static string GetWeather([Description("City name")] string city)
    {
        Console.WriteLine($"    [Tool:get_weather] city={city}");
        return $"Weather in {city}: 22°C, partly cloudy, humidity 65%";
    }

    [Description("Gets the current time for a city/timezone")]
    static string GetTime([Description("City name")] string city)
    {
        Console.WriteLine($"    [Tool:get_time] city={city}");
        return $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
    }
}

// ── Tipos complejos para Demo 3 ─────────────────────────────────────────────
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);

// Contexto de serializacion JSON seguro para NativeAOT
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
[JsonSerializable(typeof(JsonElement))]
partial class DemoJsonContext : JsonSerializerContext;
