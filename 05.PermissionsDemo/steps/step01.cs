using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new PermissionsDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class PermissionsDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "05 - DEMO: Manejo de solicitudes de permisos";
    const string Step1Text        = "Aprobar permiso - Operaciones de escritura";
    const string Step2Text        = "Denegar permiso - Bloquear modificaciones";
    const string Step3Text        = "Comportamiento por defecto (sin handler)";
    const string Step4Text        = "Handler asincrono de permisos";
    const string Step5Text        = "ToolCallId en solicitudes de permisos";
    const string Step6Text        = "Error en handler - Degradacion elegante";
    const string Step7Text        = "Handler de permisos al reanudar sesion";
    const string InteractiveHint  = "Presiona Enter para modo interactivo (aprobar/denegar cada solicitud).";
    const string InteractivePrompt = "Escribe mensajes (vacio para salir). Se te pedira aprobar/denegar cada solicitud.\n";

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

        var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
        Directory.CreateDirectory(workDir);

        await client.StopAsync();
        await client.DisposeAsync();

        try { Directory.Delete(workDir, true); } catch { /* ignore */ }
    }
}
