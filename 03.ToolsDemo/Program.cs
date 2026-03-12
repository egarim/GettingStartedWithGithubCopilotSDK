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
        await Step4_ToolErrorHandling(client);
        await Step5_ToolFilters(client);

        await client.StopAsync();
        await client.DisposeAsync();

        await RunInteractiveMode();
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

    // ── Paso 4: Manejo de errores en herramientas ───────────────────────
    async Task Step4_ToolErrorHandling(CopilotClient client)
    {
        PrintStep(4, Step4Text);
        var failingTool = AIFunctionFactory.Create(
            () => { throw new Exception("Secret Internal Error — Melbourne"); },
            "get_user_location",
            "Gets the user's location");

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [failingTool]
        });

        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "What is my location? If you can't find out, just say 'unknown'."
        });
        Console.WriteLine($"  La herramienta lanzo una excepcion con 'Melbourne' en el mensaje.");
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        Console.WriteLine($"  Contiene 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
        Console.WriteLine("  (Esperado: Falso - el SDK NO expone detalles de excepciones al modelo)");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 5: Filtros AvailableTools y ExcludedTools ──────────────────
    async Task Step5_ToolFilters(CopilotClient client)
    {
        PrintStep(5, Step5Text);

        Console.WriteLine("  AvailableTools = [\"view\", \"edit\"] -> solo estas 2 herramientas built-in");
        var session1 = await client.CreateSessionAsync(new SessionConfig
        {
            AvailableTools = new List<string> { "view", "edit" }
        });
        var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
        Console.WriteLine($"  Respuesta: {a1?.Data.Content?.Substring(0, Math.Min(200, a1.Data.Content?.Length ?? 0))}");
        await session1.DisposeAsync();

        Console.WriteLine();
        Console.WriteLine("  ExcludedTools = [\"view\"] -> 'view' excluido, los demas permanecen");
        var session2 = await client.CreateSessionAsync(new SessionConfig
        {
            ExcludedTools = new List<string> { "view" }
        });
        var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
        Console.WriteLine($"  Respuesta: {a2?.Data.Content?.Substring(0, Math.Min(200, a2.Data.Content?.Length ?? 0))}");
        await session2.DisposeAsync();
        Console.WriteLine();
    }

    // ── Modo interactivo ────────────────────────────────────────────────
    async Task RunInteractiveMode()
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {InteractiveHint}");
        Console.WriteLine("================================================================");
        Console.ReadLine();

        var client = CreateClient();
        await client.StartAsync();

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Streaming = true,
            Tools =
            [
                AIFunctionFactory.Create(EncryptString, "encrypt_string"),
                AIFunctionFactory.Create(GetWeather, "get_weather"),
                AIFunctionFactory.Create(GetTime, "get_time"),
            ]
        });

        Console.WriteLine($"  {InteractiveTools}");
        Console.WriteLine($"  {InteractivePrompt}\n");

        while (true)
        {
            Console.Write("  Tu: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;

            var done = new TaskCompletionSource<bool>();
            session.On(evt =>
            {
                if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
                if (evt is SessionIdleEvent) done.TrySetResult(true);
                if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
            });

            Console.Write("  IA: ");
            await session.SendAsync(new MessageOptions { Prompt = input });
            await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("\n");
        }

        await client.StopAsync();
        await client.DisposeAsync();
        Console.WriteLine("\n  ¡Listo!");
    }
}

// ── Implementaciones de herramientas ────────────────────────────────────────

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

// ── Tipos complejos para Demo 3 ─────────────────────────────────────────────
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);

// Contexto de serializacion JSON seguro para NativeAOT
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
[JsonSerializable(typeof(JsonElement))]
partial class DemoJsonContext : JsonSerializerContext;
