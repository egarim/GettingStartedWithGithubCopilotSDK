using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new CompactionDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class CompactionDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "07 - DEMO: Sesiones infinitas y compactacion de contexto";
    const string Step1Text        = "Compactacion activada - Umbrales bajos";
    const string Step2Text        = "Compactacion desactivada - Sin eventos";
    const string InteractiveHint  = "Presiona Enter para sesion infinita interactiva (sigue chateando para activar compactacion).";
    const string InteractivePrompt = "Sesion infinita activa - sigue chateando para llenar contexto y activar compactacion.\n";

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

        await Step1_CompactionEnabled(client);
        await Step2_CompactionDisabled(client);

        await client.StopAsync();
        await client.DisposeAsync();

        await RunInteractiveMode();
    }

    // ── Paso 1: Compactacion activada ──────────────────────────────────
    async Task Step1_CompactionEnabled(CopilotClient client)
    {
        PrintStep(1, Step1Text);
        Console.WriteLine("  (Umbrales bajos activan compactacion rapido para demostracion)\n");

        var compactionStartEvents = new List<SessionCompactionStartEvent>();
        var compactionCompleteEvents = new List<SessionCompactionCompleteEvent>();

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            InfiniteSessions = new InfiniteSessionConfig
            {
                Enabled = true,
                BackgroundCompactionThreshold = 0.005,
                BufferExhaustionThreshold = 0.01
            }
        });

        session.On(evt =>
        {
            if (evt is SessionCompactionStartEvent startEvt)
            {
                compactionStartEvents.Add(startEvt);
                Console.WriteLine("    * [CompactacionInicio] Compactacion de fondo activada!");
            }
            if (evt is SessionCompactionCompleteEvent completeEvt)
            {
                compactionCompleteEvents.Add(completeEvt);
                Console.WriteLine($"    OK [CompactacionCompleta] Exito: {completeEvt.Data.Success}, Tokens removidos: {completeEvt.Data.TokensRemoved}");
            }
        });

        Console.WriteLine("  Enviando mensaje 1/3: Historia larga de un dragon...");
        var a1 = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Tell me a long story about a dragon. Be very detailed. Include at least 5 paragraphs."
        });
        Console.WriteLine($"  Respuesta 1 longitud: {a1?.Data.Content?.Length ?? 0} chars");
        PrintProp("Eventos compactacion:", $"inicio={compactionStartEvents.Count}, completo={compactionCompleteEvents.Count}");
        Console.WriteLine();

        Console.WriteLine("  Enviando mensaje 2/3: Continuar con el castillo...");
        var a2 = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Continue the story with more details about the dragon's castle. Make it very long and descriptive."
        });
        Console.WriteLine($"  Respuesta 2 longitud: {a2?.Data.Content?.Length ?? 0} chars");
        PrintProp("Eventos compactacion:", $"inicio={compactionStartEvents.Count}, completo={compactionCompleteEvents.Count}");
        Console.WriteLine();

        Console.WriteLine("  Enviando mensaje 3/3: Describir el tesoro...");
        var a3 = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Now describe the dragon's treasure in great detail. List every item. Make this response very long."
        });
        Console.WriteLine($"  Respuesta 3 longitud: {a3?.Data.Content?.Length ?? 0} chars\n");

        Console.WriteLine("  -- Resultados de compactacion --");
        PrintProp("CompactionStart:", compactionStartEvents.Count);
        PrintProp("CompactionComplete:", compactionCompleteEvents.Count);

        if (compactionCompleteEvents.Count > 0)
        {
            var last = compactionCompleteEvents[^1];
            PrintProp("Ultima exitosa:", last.Data.Success);
            PrintProp("Tokens removidos:", last.Data.TokensRemoved);
        }
        else
        {
            Console.WriteLine("  (No se activo compactacion - la ventana de contexto puede no haberse llenado suficiente)");
        }

        Console.WriteLine();
        Console.WriteLine("  Verificando que la sesion funciona tras compactacion...");
        var a4 = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "What was the main story about? Answer in one sentence."
        });
        Console.WriteLine($"  Respuesta: {a4?.Data.Content}");
        Console.WriteLine("  (Deberia recordar que era sobre un dragon - contexto preservado via resumen)");

        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 2: Compactacion desactivada ───────────────────────────────
    async Task Step2_CompactionDisabled(CopilotClient client)
    {
        PrintStep(2, Step2Text);
        var compactionEvents = new List<SessionEvent>();

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            InfiniteSessions = new InfiniteSessionConfig
            {
                Enabled = false
            }
        });

        session.On(evt =>
        {
            if (evt is SessionCompactionStartEvent or SessionCompactionCompleteEvent)
            {
                compactionEvents.Add(evt);
            }
        });

        var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Eventos disparados:", compactionEvents.Count);
        Console.WriteLine("  (Esperado: 0 - compactacion desactivada)");

        await session.DisposeAsync();
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

        var compactionCount = 0;
        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Streaming = true,
            InfiniteSessions = new InfiniteSessionConfig
            {
                Enabled = true,
                BackgroundCompactionThreshold = 0.005,
                BufferExhaustionThreshold = 0.01
            }
        });

        session.On(evt =>
        {
            if (evt is SessionCompactionStartEvent)
                Console.WriteLine("\n    * COMPACTACION INICIADA");
            if (evt is SessionCompactionCompleteEvent c)
            {
                compactionCount++;
                Console.WriteLine($"    OK COMPACTACION #{compactionCount} COMPLETA - removidos {c.Data.TokensRemoved} tokens");
            }
        });

        Console.WriteLine($"  {InteractivePrompt}");
        while (true)
        {
            Console.Write($"  Tu [{compactionCount} compactaciones]: ");
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
