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

        await Step1_ApprovePermission(client, workDir);

        await client.StopAsync();
        await client.DisposeAsync();

        try { Directory.Delete(workDir, true); } catch { /* ignore */ }
    }

    // ── Paso 1: Aprobar permiso ────────────────────────────────────────
    async Task Step1_ApprovePermission(CopilotClient client, string workDir)
    {
        PrintStep(1, Step1Text);
        var permissionRequests = new List<PermissionRequest>();
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            OnPermissionRequest = (request, invocation) =>
            {
                permissionRequests.Add(request);
                Console.WriteLine($"    [Permission] Kind: {request.Kind}, ToolCallId: {request.ToolCallId}");
                Console.WriteLine($"    [Permission] Session: {invocation.SessionId}");
                Console.WriteLine("    [Permission] Decision: APROBADO");
                return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
            }
        });

        var testFile = Path.Combine(workDir, "test.txt");
        await File.WriteAllTextAsync(testFile, "original content");

        Console.WriteLine($"  Prompt: Edit the file at {testFile} and replace 'original' with 'modified'");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
        PrintProp("Solicitudes recibidas:", permissionRequests.Count);
        Console.WriteLine($"  Permisos de escritura: {permissionRequests.Count(r => r.Kind == "write")}");

        var content = await File.ReadAllTextAsync(testFile);
        Console.WriteLine($"  Contenido despues: \"{content}\"");
        await session.DisposeAsync();
        Console.WriteLine();
    }
}
