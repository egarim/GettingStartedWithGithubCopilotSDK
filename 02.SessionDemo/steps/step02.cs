using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new SessionDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class SessionDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "02 - DEMO: Ciclo de vida, eventos y multi-turno";
    const string Step1Text        = "Crear y destruir una sesion";
    const string Step2Text        = "Conversacion con estado multi-turno";
    const string Step3Text        = "Suscripcion a eventos (session.On)";
    const string Step4Text        = "SendAsync (disparar y olvidar)";
    const string Step5Text        = "SendAndWaitAsync (bloquea hasta idle)";
    const string Step6Text        = "Reanudar sesion (ResumeSessionAsync)";
    const string Step7Text        = "Reanudar sesion inexistente (manejo de errores)";
    const string Step8Text        = "Mensaje de sistema - Modo Append";
    const string Step9Text        = "Mensaje de sistema - Modo Replace";
    const string Step10Text       = "Deltas en streaming";
    const string InteractiveHint  = "Presiona Enter para chat interactivo, o Ctrl+C para salir.";
    const string InteractivePrompt = "Escribe mensajes (linea vacia para salir):";

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

    // ── Helper: esperar idle ────────────────────────────────────────────
    async Task<AssistantMessageEvent?> WaitForIdleAsync(CopilotSession session, int timeoutSeconds = 60)
    {
        AssistantMessageEvent? lastMessage = null;
        var tcs = new TaskCompletionSource<bool>();

        var sub = session.On(evt =>
        {
            if (evt is AssistantMessageEvent msg) lastMessage = msg;
            if (evt is SessionIdleEvent) tcs.TrySetResult(true);
            if (evt is SessionErrorEvent err) tcs.TrySetException(new Exception(err.Data?.Message));
        });

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds));
        sub.Dispose();
        return lastMessage;
    }

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var client = CreateClient();
        await client.StartAsync();
        Console.WriteLine("  Cliente iniciado.\n");

        await Step1_CreateAndDestroy(client);

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Paso 1: Crear y destruir una sesion ─────────────────────────────
    async Task Step1_CreateAndDestroy(CopilotClient client)
    {
        PrintStep(1, Step1Text);
        var session = await client.CreateSessionAsync(new SessionConfig { Model = "gpt-4o" });
        PrintProp("Sesion creada:", session.SessionId);

        var messages = await session.GetMessagesAsync();
        PrintProp("Mensajes iniciales:", messages.Count);
        PrintProp("Tipo primer evento:", messages[0].GetType().Name);

        await session.DisposeAsync();
        Console.WriteLine("  Sesion destruida.");

        try
        {
            await session.GetMessagesAsync();
        }
        catch (IOException ex)
        {
            PrintProp("Error esperado:", ex.Message);
        }
        Console.WriteLine();
    }
}
