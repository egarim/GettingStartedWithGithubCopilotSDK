using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

// ============================================================================
// 03 – TOOLS DEMO: Custom Tools (AIFunction)
// 03 – DEMO DE HERRAMIENTAS: Herramientas personalizadas (AIFunction)
//
// Demonstrates / Demuestra:
//   • Simple custom tool with AIFunctionFactory.Create
//   • [Description] attributes on methods and parameters
//   • Complex input/output types (records, arrays)
//   • NativeAOT-safe JsonSerializerContext
//   • Tool error handling (exceptions in tools)
//   • Accessing ToolInvocation from AIFunctionArguments.Context
// ============================================================================

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Language selection
Console.WriteLine("================================================================");
Console.WriteLine("  Select language / Seleccione idioma:");
Console.WriteLine("  1. English");
Console.WriteLine("  2. Español");
Console.WriteLine("================================================================");
Console.Write("  Choice (1 or 2): ");
var langChoice = Console.ReadLine()?.Trim();
bool isSpanish = langChoice == "2";
Console.WriteLine();

Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  03 - DEMO: Herramientas personalizadas (AIFunction)");
}
else
{
    Console.WriteLine("  03 - TOOLS DEMO: Custom Tools (AIFunction)");
}
Console.WriteLine("================================================================");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine(isSpanish ? "  Cliente iniciado.\n" : "  Client started.\n");

// -- 1. Simple Custom Tool / Herramienta personalizada simple ---------
if (isSpanish)
{
    Console.WriteLine("=== 1. Herramienta personalizada simple (encrypt_string) ===");
}
else
{
    Console.WriteLine("=== 1. Simple Custom Tool (encrypt_string) ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
    });

    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "Use encrypt_string to encrypt this string: Hello World" });
    Console.WriteLine($"  Prompt:   Use encrypt_string to encrypt: Hello World");
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine("  (The tool converts to uppercase — the model should include 'HELLO WORLD')");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. Multiple Tools / Multiples herramientas -----------------------
if (isSpanish)
{
    Console.WriteLine("=== 2. Multiples herramientas personalizadas ===");
}
else
{
    Console.WriteLine("=== 2. Multiple Custom Tools ===");
}
{
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
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 3. Complex Input/Output Types / Tipos complejos ------------------
if (isSpanish)
{
    Console.WriteLine("=== 3. Tipos complejos de entrada/salida (records, arrays) ===");
}
else
{
    Console.WriteLine("=== 3. Complex Input/Output Types (records, arrays) ===");
}
{
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
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  ToolInvocation.SessionId matches: {receivedInvocation?.SessionId == session.SessionId}");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 4. Tool Error Handling / Manejo de errores en herramientas -------
if (isSpanish)
{
    Console.WriteLine("=== 4. Manejo de errores en herramientas ===");
}
else
{
    Console.WriteLine("=== 4. Tool Error Handling ===");
}
{
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
    Console.WriteLine($"  The tool threw an exception with 'Melbourne' in the message.");
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  Contains 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
    Console.WriteLine(isSpanish
        ? "  (Esperado: Falso - el SDK NO expone detalles de excepciones al modelo)"
        : "  (Expected: False - SDK does NOT leak exception details to the model)");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 5. Tool with AvailableTools filter / Filtro de herramientas ------
if (isSpanish)
{
    Console.WriteLine("=== 5. Filtros AvailableTools y ExcludedTools ===");
}
else
{
    Console.WriteLine("=== 5. AvailableTools & ExcludedTools filters ===");
}
{
    Console.WriteLine("  AvailableTools = [\"view\", \"edit\"] → only these 2 built-in tools allowed");
    var session1 = await client.CreateSessionAsync(new SessionConfig
    {
        AvailableTools = new List<string> { "view", "edit" }
    });
    var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
    Console.WriteLine($"  Response: {a1?.Data.Content?.Substring(0, Math.Min(200, a1.Data.Content?.Length ?? 0))}");
    await session1.DisposeAsync();

    Console.WriteLine();
    Console.WriteLine(isSpanish
        ? "  ExcludedTools = [\"view\"] -> 'view' excluido, los demas permanecen"
        : "  ExcludedTools = [\"view\"] -> 'view' is excluded, others remain");
    var session2 = await client.CreateSessionAsync(new SessionConfig
    {
        ExcludedTools = new List<string> { "view" }
    });
    var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What tools do you have available?" });
    Console.WriteLine($"  Response: {a2?.Data.Content?.Substring(0, Math.Min(200, a2.Data.Content?.Length ?? 0))}");
    await session2.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para modo interactivo con herramientas, o Ctrl+C para salir."
    : "  Press Enter for interactive mode with tools, or Ctrl+C to exit.");
Console.WriteLine("================================================================");
Console.ReadLine();

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();

await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    Tools =
    [
        AIFunctionFactory.Create(EncryptString, "encrypt_string"),
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

Console.WriteLine(isSpanish
    ? "  Herramientas: encrypt_string, get_weather, get_time"
    : "  Available tools: encrypt_string, get_weather, get_time");
Console.WriteLine(isSpanish ? "  Escribe mensajes (vacio para salir):\n" : "  Type messages (empty to quit):\n");

while (true)
{
    Console.Write("  You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    chatSession.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
        if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
    });

    Console.Write("  AI: ");
    await chatSession.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine();
}

await chatClient.StopAsync();
await chatClient.DisposeAsync();
Console.WriteLine(isSpanish ? "\n  ¡Listo!" : "\n  Done!");

// -- Tool implementations / Implementaciones de herramientas -----------

[Description("Encrypts a string by converting it to uppercase")]
// Cifra una cadena convirtiéndola a mayúsculas
static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();

[Description("Gets the current weather for a city")]
// Obtiene el clima actual de una ciudad
static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"    [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 65%";
}

[Description("Gets the current time for a city/timezone")]
// Obtiene la hora actual de una ciudad/zona horaria
static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"    [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
}

// -- Complex types for Demo 3 / Tipos complejos para Demo 3 -----------
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);

// NativeAOT-safe JSON serialization context
// Contexto de serialización JSON seguro para NativeAOT
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
[JsonSerializable(typeof(JsonElement))]
partial class DemoJsonContext : JsonSerializerContext;
