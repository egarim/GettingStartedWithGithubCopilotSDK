using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new McpAgentsDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class McpAgentsDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "09 - DEMO: Servidores MCP y agentes personalizados";
    const string Step1Text        = "Configuracion de un servidor MCP";
    const string Step2Text        = "Multiples servidores MCP";
    const string Step3Text        = "Configuracion de agente personalizado";
    const string Step4Text        = "Agente con herramientas especificas";
    const string Step5Text        = "Agente con sus propios servidores MCP";
    const string Step6Text        = "Multiples agentes personalizados";
    const string Step7Text        = "Combinacion: Servidores MCP + Agentes";
    const string Step8Text        = "MCP y Agentes al reanudar sesion";
    const string InteractiveHint  = "Presiona Enter para modo interactivo con un agente personalizado.";

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
}
