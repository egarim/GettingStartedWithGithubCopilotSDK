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

        await Step1_SimpleCustomTool(client);
        await Step2_MultipleTools(client);
        await Step3_ComplexTypes(client);

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Paso 1: Herramienta personalizada simple ────────────────────────
    async Task Step1_SimpleCustomTool(CopilotClient client)
    {
        PrintStep(1, Step1Text);
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
        });

        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "Use encrypt_string to encrypt this string: Hello World" });
        Console.WriteLine($"  Prompt:   Use encrypt_string to encrypt: Hello World");
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        Console.WriteLine("  (La herramienta convierte a mayusculas — el modelo deberia incluir 'HELLO WORLD')");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 2: Multiples herramientas ──────────────────────────────────
    async Task Step2_MultipleTools(CopilotClient client)
    {
        PrintStep(2, Step2Text);
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
        Console.WriteLine($"  Prompt:   What's the weather in Madrid and what time is it there?");
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 3: Tipos complejos de entrada/salida ───────────────────────
    async Task Step3_ComplexTypes(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        ToolInvocation? receivedInvocation = null;

        City[] PerformDbQuery(DbQueryOptions query, AIFunctionArguments rawArgs)
        {
            Console.WriteLine($"    [Tool called] Table={query.Table}, IDs=[{string.Join(",", query.Ids)}], Sort={query.SortAscending}");
            receivedInvocation = (ToolInvocation)rawArgs.Context![typeof(ToolInvocation)]!;
            return [new(19, "Passos", 135460), new(12, "San Lorenzo", 204356)];
        }

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [AIFunctionFactory.Create(PerformDbQuery, "db_query",
                serializerOptions: DemoJsonContext.Default.Options)]
        });

        Console.WriteLine("  Prompt: Perform a DB query for the 'cities' table using IDs 12 and 19, sorting ascending.");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Perform a DB query for the 'cities' table using IDs 12 and 19, sorting ascending. " +
                     "Reply only with lines of the form: [cityname] [population]"
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        Console.WriteLine($"  ToolInvocation.SessionId coincide: {receivedInvocation?.SessionId == session.SessionId}");
        await session.DisposeAsync();
        Console.WriteLine();
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
