using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 07 – COMPACTION DEMO: Infinite Sessions & Context Compaction
// 07 – DEMO COMPACTACIÓN: Sesiones infinitas y compactación de contexto
//
// Demonstrates / Demuestra:
//   • InfiniteSessionConfig — enable infinite sessions
//   • BackgroundCompactionThreshold / BufferExhaustionThreshold
//   • SessionCompactionStartEvent / SessionCompactionCompleteEvent
//   • Token removal tracking (TokensRemoved)
//   • Disabled compaction — no compaction events
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
    Console.WriteLine("  07 - DEMO: Sesiones infinitas y compactacion de contexto");
}
else
{
    Console.WriteLine("  07 - COMPACTION DEMO: Infinite Sessions & Compaction");
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

// -- 1. Compaction Enabled (low thresholds) / Compactacion activada ----
if (isSpanish)
{
    Console.WriteLine("=== 1. Compactacion activada - Umbrales bajos ===");
    Console.WriteLine("  (Umbrales bajos activan compactacion rapido para demostracion)");
}
else
{
    Console.WriteLine("=== 1. Compaction Enabled - Low Thresholds ===");
    Console.WriteLine("  (Low thresholds trigger compaction quickly for demo purposes)");
}
Console.WriteLine();
{
    var compactionStartEvents = new List<SessionCompactionStartEvent>();
    var compactionCompleteEvents = new List<SessionCompactionCompleteEvent>();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        InfiniteSessions = new InfiniteSessionConfig
        {
            Enabled = true,
            // Very low thresholds to trigger compaction quickly
            // Umbrales muy bajos para activar compactación rápidamente
            BackgroundCompactionThreshold = 0.005, // 0.5% of context → ~1000 tokens
            BufferExhaustionThreshold = 0.01        // 1% → block and compact
        }
    });

    session.On(evt =>
    {
        if (evt is SessionCompactionStartEvent startEvt)
        {
            compactionStartEvents.Add(startEvt);
            Console.WriteLine(isSpanish
                ? "    * [CompactacionInicio] ¡Compactacion de fondo activada!"
                : "    * [CompactionStart] Background compaction triggered!");
        }
        if (evt is SessionCompactionCompleteEvent completeEvt)
        {
            compactionCompleteEvents.Add(completeEvt);
            Console.WriteLine(isSpanish
                ? $"    OK [CompactacionCompleta] Exito: {completeEvt.Data.Success}, Tokens removidos: {completeEvt.Data.TokensRemoved}"
                : $"    OK [CompactionComplete] Success: {completeEvt.Data.Success}, Tokens removed: {completeEvt.Data.TokensRemoved}");
        }
    });

    // Send multiple long messages to fill up the context window
    // Enviar múltiples mensajes largos para llenar la ventana de contexto
    Console.WriteLine("  Sending message 1/3: Tell me a long story about a dragon...");
    var a1 = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Tell me a long story about a dragon. Be very detailed. Include at least 5 paragraphs."
    });
    Console.WriteLine($"  Response 1 length: {a1?.Data.Content?.Length ?? 0} chars");
    Console.WriteLine($"  Compaction events so far: start={compactionStartEvents.Count}, complete={compactionCompleteEvents.Count}");
    Console.WriteLine();

    Console.WriteLine("  Sending message 2/3: Continue the story with the castle...");
    var a2 = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Continue the story with more details about the dragon's castle. Make it very long and descriptive."
    });
    Console.WriteLine($"  Response 2 length: {a2?.Data.Content?.Length ?? 0} chars");
    Console.WriteLine($"  Compaction events so far: start={compactionStartEvents.Count}, complete={compactionCompleteEvents.Count}");
    Console.WriteLine();

    Console.WriteLine("  Sending message 3/3: Describe the dragon's treasure...");
    var a3 = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Now describe the dragon's treasure in great detail. List every item. Make this response very long."
    });
    Console.WriteLine($"  Response 3 length: {a3?.Data.Content?.Length ?? 0} chars");
    Console.WriteLine();

    // Results / Resultados
    Console.WriteLine(isSpanish
        ? "  -- Resultados de compactacion --"
        : "  -- Compaction Results --");
    Console.WriteLine($"  CompactionStart events:    {compactionStartEvents.Count}");
    Console.WriteLine($"  CompactionComplete events: {compactionCompleteEvents.Count}");

    if (compactionCompleteEvents.Count > 0)
    {
        var last = compactionCompleteEvents[^1];
        Console.WriteLine($"  Last compaction success:   {last.Data.Success}");
        Console.WriteLine($"  Last tokens removed:       {last.Data.TokensRemoved}");
    }
    else
    {
        Console.WriteLine(isSpanish
            ? "  (No se activo compactacion - la ventana de contexto puede no haberse llenado suficiente)"
            : "  (No compaction was triggered - context window may not have been filled enough)");
    }

    // Verify session still works after compaction
    // Verificar que la sesión aún funciona tras compactación
    Console.WriteLine();
    Console.WriteLine("  Verifying session works after compaction...");
    var a4 = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "What was the main story about? Answer in one sentence."
    });
    Console.WriteLine($"  Response: {a4?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  (Deberia recordar que era sobre un dragon - contexto preservado via resumen)"
        : "  (Should still remember it was about a dragon - context preserved via summary)");

    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. Compaction Disabled / Compactacion desactivada -----------------
if (isSpanish)
{
    Console.WriteLine("=== 2. Compactacion desactivada - Sin eventos ===");
}
else
{
    Console.WriteLine("=== 2. Compaction Disabled - No Events ===");
}
{
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
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  Compaction events fired: {compactionEvents.Count}");
    Console.WriteLine(isSpanish
        ? "  (Esperado: 0 - compactacion desactivada)"
        : "  (Expected: 0 - compaction is disabled)");

    await session.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para sesion infinita interactiva (¡sigue chateando para activar compactacion!)."
    : "  Press Enter for interactive infinite session (keep chatting to trigger compaction!).");
Console.WriteLine("================================================================");
Console.ReadLine();

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();

var compactionCount = 0;
await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.005,
        BufferExhaustionThreshold = 0.01
    }
});

chatSession.On(evt =>
{
    if (evt is SessionCompactionStartEvent)
        Console.WriteLine(isSpanish
            ? "\n    * COMPACTACION INICIADA"
            : "\n    * COMPACTION STARTED");
    if (evt is SessionCompactionCompleteEvent c)
    {
        compactionCount++;
        Console.WriteLine(isSpanish
            ? $"    OK COMPACTACION #{compactionCount} COMPLETA - removidos {c.Data.TokensRemoved} tokens"
            : $"    OK COMPACTION #{compactionCount} COMPLETE - removed {c.Data.TokensRemoved} tokens");
    }
});

Console.WriteLine(isSpanish
    ? "  Sesion infinita activa - sigue chateando para llenar contexto y activar compactacion.\n"
    : "  Infinite session active - keep chatting to fill context and trigger compaction.\n");

while (true)
{
    Console.Write($"  You [{compactionCount} compactions]: ");
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
