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
        await Step2_MultiTurn(client);
        await Step3_EventSubscription(client);
        await Step4_SendAsync(client);

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

    // ── Paso 2: Conversacion con estado multi-turno ─────────────────────
    async Task Step2_MultiTurn(CopilotClient client)
    {
        PrintStep(2, Step2Text);
        await using var session = await client.CreateSessionAsync();

        var answer1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
        Console.WriteLine($"  Q1: What is 10 + 15?");
        Console.WriteLine($"  A1: {answer1?.Data.Content?.Substring(0, Math.Min(150, answer1.Data.Content?.Length ?? 0))}");

        var answer2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
        Console.WriteLine($"  Q2: Now double that result.");
        Console.WriteLine($"  A2: {answer2?.Data.Content?.Substring(0, Math.Min(150, answer2.Data.Content?.Length ?? 0))}");
        Console.WriteLine("  (El modelo recuerda la respuesta anterior)");
        Console.WriteLine();
    }

    // ── Paso 3: Suscripcion a eventos ───────────────────────────────────
    async Task Step3_EventSubscription(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        await using var session = await client.CreateSessionAsync();
        var receivedEvents = new List<string>();
        var idleTcs = new TaskCompletionSource<bool>();

        var sub = session.On(evt =>
        {
            receivedEvents.Add(evt.GetType().Name);
            if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
        });

        Console.WriteLine("  Prompt: What is 100 + 200?");
        await session.SendAsync(new MessageOptions { Prompt = "What is 100 + 200?" });
        await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
        sub.Dispose();

        Console.WriteLine("  Eventos recibidos:");
        foreach (var e in receivedEvents)
            Console.WriteLine($"    • {e}");
        Console.WriteLine();
    }

    // ── Paso 4: SendAsync (disparar y olvidar) ──────────────────────────
    async Task Step4_SendAsync(CopilotClient client)
    {
        PrintStep(4, Step4Text);
        await using var session = await client.CreateSessionAsync();
        var events = new List<string>();
        session.On(evt => events.Add(evt.Type));

        Console.WriteLine("  Prompt: What is 2+2?");
        await session.SendAsync(new MessageOptions { Prompt = "What is 2+2?" });
        Console.WriteLine($"  Despues de SendAsync -> session.idle en events? {events.Contains("session.idle")}");
        Console.WriteLine("  (Esperado: False - SendAsync retorna antes de que termine el turno)");

        var finalMsg = await WaitForIdleAsync(session);
        Console.WriteLine($"  Despues de esperar -> session.idle en events? {events.Contains("session.idle")}");
        Console.WriteLine($"  Respuesta: {finalMsg?.Data.Content}");
        Console.WriteLine();
    }
}
