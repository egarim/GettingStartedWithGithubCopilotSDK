using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

// ============================================================================
// 02 – SESSION DEMO: Session Lifecycle, Events & Multi-turn
// 02 – DEMO DE SESIÓN: Ciclo de vida, eventos y conversación multi-turno
//
// Demonstrates / Demuestra:
//   • Create & destroy sessions
//   • Multi-turn stateful conversations
//   • Event subscription (On)
//   • SendAsync vs SendAndWaitAsync
//   • Session resume (ResumeSessionAsync)
//   • Session abort (AbortAsync)
//   • Streaming deltas
//   • System message configuration (Append / Replace)
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
    Console.WriteLine("  02 - DEMO: Ciclo de vida, eventos y multi-turno");
}
else
{
    Console.WriteLine("  02 - SESSION DEMO: Lifecycle, Events & Multi-turn");
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

// -- 1. Create & Destroy Session / Crear y destruir sesion --
Console.WriteLine(isSpanish
    ? "=== 1. Crear y destruir una sesion ==="
    : "=== 1. Create & Destroy Session ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig { Model = "gpt-4o" });
    Console.WriteLine(isSpanish
        ? $"  Sesión creada: {session.SessionId}"
        : $"  Session created: {session.SessionId}");

    var messages = await session.GetMessagesAsync();
    Console.WriteLine(isSpanish
        ? $"  Mensajes iniciales: {messages.Count}"
        : $"  Initial messages: {messages.Count}");
    Console.WriteLine(isSpanish
        ? $"  Tipo primer evento: {messages[0].GetType().Name}"
        : $"  First event type: {messages[0].GetType().Name}");

    await session.DisposeAsync();
    Console.WriteLine(isSpanish
        ? "  Sesión destruida."
        : "  Session disposed.");

    try
    {
        await session.GetMessagesAsync();
    }
    catch (IOException ex)
    {
        Console.WriteLine(isSpanish
            ? $"  Error esperado: {ex.Message}"
            : $"  Expected error after dispose: {ex.Message}");
    }
}
Console.WriteLine();

// -- 2. Multi-turn Stateful Conversation / Conversacion con estado --
Console.WriteLine(isSpanish
    ? "=== 2. Conversacion con estado multi-turno ==="
    : "=== 2. Multi-turn Stateful Conversation ===");
{
    await using var session = await client.CreateSessionAsync();

    var answer1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
    Console.WriteLine($"  Q1: What is 10 + 15?");
    Console.WriteLine($"  A1: {answer1?.Data.Content?.Substring(0, Math.Min(150, answer1.Data.Content?.Length ?? 0))}");

    var answer2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
    Console.WriteLine($"  Q2: Now double that result.");
    Console.WriteLine($"  A2: {answer2?.Data.Content?.Substring(0, Math.Min(150, answer2.Data.Content?.Length ?? 0))}");
    Console.WriteLine(isSpanish
        ? "  (El modelo recuerda la respuesta anterior)"
        : "  (The model remembers the previous answer)");
}
Console.WriteLine();

// -- 3. Event Subscription / Suscripcion a eventos --
Console.WriteLine(isSpanish
    ? "=== 3. Suscripcion a eventos (session.On) ==="
    : "=== 3. Event Subscription (session.On) ===");
{
    await using var session = await client.CreateSessionAsync();
    var receivedEvents = new List<string>();
    var idleTcs = new TaskCompletionSource<bool>();

    var sub = session.On(evt =>
    {
        receivedEvents.Add(evt.GetType().Name);
        if (evt is SessionIdleEvent) idleTcs.TrySetResult(true);
    });

    await session.SendAsync(new MessageOptions { Prompt = "What is 100 + 200?" });
    await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
    sub.Dispose();

    Console.WriteLine(isSpanish
        ? "  Eventos recibidos:"
        : "  Events received:");
    foreach (var e in receivedEvents)
        Console.WriteLine($"    • {e}");
}
Console.WriteLine();

// -- 4. SendAsync vs SendAndWaitAsync --
Console.WriteLine(isSpanish
    ? "=== 4. SendAsync (disparar y olvidar) vs SendAndWaitAsync (bloqueante) ==="
    : "=== 4. SendAsync (fire-and-forget) vs SendAndWaitAsync (blocking) ===");
{
    await using var session = await client.CreateSessionAsync();
    var events = new List<string>();
    session.On(evt => events.Add(evt.Type));

    // SendAsync returns immediately / SendAsync retorna inmediatamente
    await session.SendAsync(new MessageOptions { Prompt = "What is 2+2?" });
    Console.WriteLine($"  After SendAsync -> session.idle in events? {events.Contains("session.idle")}");
    Console.WriteLine(isSpanish
        ? "  (Esperado: False - SendAsync retorna antes de que termine el turno)"
        : "  (Expected: False - SendAsync returns before turn completes)");

    // Wait for idle / Esperar a idle
    var finalMsg = await WaitForIdleAsync(session);
    Console.WriteLine($"  After waiting -> session.idle in events? {events.Contains("session.idle")}");
    Console.WriteLine($"  Response: {finalMsg?.Data.Content}");
}
Console.WriteLine();

// -- 5. SendAndWaitAsync (blocks until idle) --
Console.WriteLine(isSpanish
    ? "=== 5. SendAndWaitAsync (bloquea hasta idle) ==="
    : "=== 5. SendAndWaitAsync (blocks until idle) ===");
{
    await using var session = await client.CreateSessionAsync();
    var events = new List<string>();
    session.On(evt => events.Add(evt.Type));

    var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
    Console.WriteLine($"  Response: {response?.Data.Content}");
    Console.WriteLine($"  Events after return: {string.Join(", ", events.Distinct())}");
    Console.WriteLine(isSpanish
        ? "  (session.idle ya esta en events porque SendAndWaitAsync bloquea)"
        : "  (session.idle is already in events because SendAndWaitAsync blocks)");
}
Console.WriteLine();

// -- 6. Session Resume / Reanudar sesion --
Console.WriteLine(isSpanish
    ? "=== 6. Reanudar sesion (ResumeSessionAsync) ==="
    : "=== 6. Session Resume (ResumeSessionAsync) ===");
{
    // Create session and send a message
    var session1 = await client.CreateSessionAsync();
    var sessionId = session1.SessionId;
    var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "Remember this number: 42" });
    Console.WriteLine($"  Session 1 (ID: {sessionId}): {a1?.Data.Content?.Substring(0, Math.Min(100, a1.Data.Content?.Length ?? 0))}");

    // Resume the same session
    var session2 = await client.ResumeSessionAsync(sessionId);
    Console.WriteLine(isSpanish
        ? $"  ID de sesion reanudada coincide: {session2.SessionId == sessionId}"
        : $"  Resumed session ID matches: {session2.SessionId == sessionId}");

    var messages = await session2.GetMessagesAsync();
    Console.WriteLine(isSpanish
        ? $"  Mensajes despues de reanudar: {messages.Count}"
        : $"  Messages after resume: {messages.Count}");
    Console.WriteLine($"  Contains SessionResumeEvent: {messages.Any(m => m is SessionResumeEvent)}");

    var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What number did I ask you to remember?" });
    Console.WriteLine($"  Session 2 response: {a2?.Data.Content?.Substring(0, Math.Min(100, a2.Data.Content?.Length ?? 0))}");

    await session2.DisposeAsync();
}
Console.WriteLine();

// -- 7. Resume non-existent session (error) --
Console.WriteLine(isSpanish
    ? "=== 7. Reanudar sesion inexistente (manejo de errores) ==="
    : "=== 7. Resume Non-Existent Session (error handling) ===");
{
    try
    {
        await client.ResumeSessionAsync("non-existent-session-id");
    }
    catch (IOException ex)
    {
        Console.WriteLine(isSpanish
            ? $"  Error esperado: {ex.Message}"
            : $"  Expected error: {ex.Message}");
    }
}
Console.WriteLine();

// -- 8. System Message (Append mode) / Mensaje de sistema --
Console.WriteLine(isSpanish
    ? "=== 8. Mensaje de sistema - Modo Append ==="
    : "=== 8. System Message - Append Mode ===");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        SystemMessage = new SystemMessageConfig
        {
            Mode = SystemMessageMode.Append,
            Content = "End each response with the phrase 'Have a nice day!'"
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your name?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  Contains 'Have a nice day!': {answer?.Data.Content?.Contains("Have a nice day!") ?? false}");
}
Console.WriteLine();

// -- 9. System Message (Replace mode) / Reemplazar mensaje de sistema --
Console.WriteLine(isSpanish
    ? "=== 9. Mensaje de sistema - Modo Replace ==="
    : "=== 9. System Message - Replace Mode ===");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        SystemMessage = new SystemMessageConfig
        {
            Mode = SystemMessageMode.Replace,
            Content = "You are an assistant called Testy McTestface. Reply succinctly."
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your full name?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  (Deberia mencionar 'Testy' en lugar de 'GitHub Copilot')"
        : "  (Should mention 'Testy' instead of 'GitHub Copilot')");
}
Console.WriteLine();

// -- 10. Streaming Deltas / Deltas en streaming --
Console.WriteLine(isSpanish
    ? "=== 10. Deltas en streaming ==="
    : "=== 10. Streaming Deltas ===");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig { Streaming = true });
    var buffer = new StringBuilder();
    var idleTcs = new TaskCompletionSource<bool>();

    session.On(evt =>
    {
        switch (evt)
        {
            case AssistantMessageDeltaEvent delta:
                Console.Write(delta.Data.DeltaContent);
                buffer.Append(delta.Data.DeltaContent);
                break;
            case SessionIdleEvent:
                idleTcs.TrySetResult(true);
                break;
        }
    });

    Console.Write("  ");
    await session.SendAsync(new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
    await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
    Console.WriteLine();
    Console.WriteLine(isSpanish
        ? $"  Caracteres transmitidos: {buffer.Length}"
        : $"  Total streamed chars: {buffer.Length}");
}
Console.WriteLine();

// ── Cleanup / Limpieza ───────────────────────────────────────────────
await client.StopAsync();
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo --
Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  Presiona Enter para chat interactivo, o Ctrl+C para salir.");
}
else
{
    Console.WriteLine("  Press Enter for interactive chat, or Ctrl+C to exit.");
}
Console.WriteLine("================================================================");
Console.ReadLine();

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();
await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig { Streaming = true });

Console.WriteLine(isSpanish
    ? "  Escribe mensajes (linea vacia para salir):"
    : "  Type messages (empty line to quit):");
while (true)
{
    Console.Write("\n  You: ");
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

// -- Helper / Ayudante --────
static async Task<AssistantMessageEvent?> WaitForIdleAsync(CopilotSession session, int timeoutSeconds = 60)
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
