using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 05 – PERMISSIONS DEMO: Permission Request Handling
// 05 – DEMO DE PERMISOS: Manejo de solicitudes de permisos
//
// Demonstrates / Demuestra:
//   • OnPermissionRequest handler — approve write operations
//   • Deny permission — block file modifications
//   • Default behavior (no handler)
//   • Async permission handler
//   • ToolCallId inspection
//   • Handler error graceful degradation
//   • Permission handler on session resume
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
    Console.WriteLine("  05 - DEMO: Manejo de solicitudes de permisos");
}
else
{
    Console.WriteLine("  05 - PERMISSIONS DEMO: Permission Request Handling");
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

// Create a temp working directory for file operations
// Crear un directorio temporal para operaciones de archivo
var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
Directory.CreateDirectory(workDir);

// -- 1. Approve Permission / Aprobar permiso ---------------------------
if (isSpanish)
{
    Console.WriteLine("=== 1. Aprobar permiso - Operaciones de escritura ===");
}
else
{
    Console.WriteLine("=== 1. Approve Permission - Write Operations ===");
}
{
    var permissionRequests = new List<PermissionRequest>();
    CopilotSession? session = null;
    session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            permissionRequests.Add(request);
            Console.WriteLine($"    [Permission] Kind: {request.Kind}, ToolCallId: {request.ToolCallId}");
            Console.WriteLine($"    [Permission] Session: {invocation.SessionId}");
            Console.WriteLine(isSpanish
                ? "    [Permission] Decision: APROBADO"
                : "    [Permission] Decision: APPROVED");
            return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
        }
    });

    // Create a test file / Crear archivo de prueba
    var testFile = Path.Combine(workDir, "test.txt");
    await File.WriteAllTextAsync(testFile, "original content");

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
    });
    Console.WriteLine($"  Response: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
    Console.WriteLine(isSpanish
        ? $"  Solicitudes recibidas: {permissionRequests.Count}"
        : $"  Permission requests received: {permissionRequests.Count}");
    Console.WriteLine($"  Write permissions: {permissionRequests.Count(r => r.Kind == "write")}");

    // Check if file was modified / Verificar si se modificó
    var content = await File.ReadAllTextAsync(testFile);
    Console.WriteLine(isSpanish
        ? $"  Contenido despues: \"{content}\""
        : $"  File content after: \"{content}\"");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. Deny Permission / Denegar permiso ------------------------------
if (isSpanish)
{
    Console.WriteLine("=== 2. Denegar permiso - Bloquear modificaciones ===");
}
else
{
    Console.WriteLine("=== 2. Deny Permission - Block Modifications ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            Console.WriteLine(isSpanish
                ? $"    [Permission] Kind: {request.Kind} -> DENEGADO"
                : $"    [Permission] Kind: {request.Kind} -> DENIED");
            return Task.FromResult(new PermissionRequestResult
            {
                Kind = "denied-interactively-by-user"
            });
        }
    });

    var protectedFile = Path.Combine(workDir, "protected.txt");
    await File.WriteAllTextAsync(protectedFile, "protected content");

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = $"Edit the file at {protectedFile} and replace 'protected' with 'hacked'"
    });
    Console.WriteLine($"  Response: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");

    var content = await File.ReadAllTextAsync(protectedFile);
    Console.WriteLine($"  File content (should be unchanged): \"{content}\"");
    Console.WriteLine($"  File protected: {content == "protected content"} ✓");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 3. Default Behavior (no handler) / Comportamiento por defecto -----
if (isSpanish)
{
    Console.WriteLine("=== 3. Comportamiento por defecto (sin handler de permisos) ===");
}
else
{
    Console.WriteLine("=== 3. Default Behavior (no permission handler) ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig());

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  (Funciona sin handler - solo se activa para operaciones de escritura/ejecucion)"
        : "  (Works fine without a permission handler - only triggered for write/run ops)");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 4. Async Permission Handler / Handler asincrono -------------------
if (isSpanish)
{
    Console.WriteLine("=== 4. Handler asincrono de permisos (retraso simulado) ===");
}
else
{
    Console.WriteLine("=== 4. Async Permission Handler (simulated delay) ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = async (request, invocation) =>
        {
            Console.WriteLine(isSpanish
                ? $"    [Permission] Kind: {request.Kind} - Simulando verificacion asincrona..."
                : $"    [Permission] Kind: {request.Kind} - simulating async check...");
            await Task.Delay(500); // Simulate async lookup / Simular consulta asincrona
            Console.WriteLine(isSpanish
                ? "    [Permission] Aprobado tras espera"
                : "    [Permission] Approved after delay");
            return new PermissionRequestResult { Kind = "approved" };
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Run 'echo hello from async permission demo' and tell me the output"
    });
    Console.WriteLine($"  Response: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 5. ToolCallId Inspection / Inspeccion de ToolCallId ---------------
if (isSpanish)
{
    Console.WriteLine("=== 5. ToolCallId en solicitudes de permisos ===");
}
else
{
    Console.WriteLine("=== 5. ToolCallId in Permission Requests ===");
}
{
    var toolCallIds = new List<string>();
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            if (!string.IsNullOrEmpty(request.ToolCallId))
            {
                toolCallIds.Add(request.ToolCallId);
                Console.WriteLine($"    [Permission] ToolCallId: {request.ToolCallId}");
            }
            return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Run 'echo test-toolcallid'"
    });
    Console.WriteLine($"  Response: {answer?.Data.Content?.Substring(0, Math.Min(150, answer?.Data.Content?.Length ?? 0))}");
    Console.WriteLine($"  ToolCallIds received: {toolCallIds.Count}");
    foreach (var id in toolCallIds) Console.WriteLine($"    → {id}");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 6. Handler Error Graceful Degradation / Degradacion elegante ------
if (isSpanish)
{
    Console.WriteLine("=== 6. Error en handler - Degradacion elegante ===");
}
else
{
    Console.WriteLine("=== 6. Handler Error - Graceful Degradation ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            Console.WriteLine(isSpanish
                ? "    [Permission] ¡A punto de LANZAR excepcion!"
                : "    [Permission] About to THROW an exception!");
            throw new InvalidOperationException("Simulated handler crash");
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Run 'echo test'. If you can't, say 'failed'."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
    Console.WriteLine(isSpanish
        ? "  (El SDK maneja la excepcion elegantemente - permiso denegado automaticamente)"
        : "  (SDK handles the exception gracefully - permission denied automatically)");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 7. Permission on Session Resume / Permisos al reanudar ------------
if (isSpanish)
{
    Console.WriteLine("=== 7. Handler de permisos al reanudar sesion ===");
}
else
{
    Console.WriteLine("=== 7. Permission Handler on Session Resume ===");
}
{
    var session1 = await client.CreateSessionAsync();
    var sessionId = session1.SessionId;
    await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What is 1+1?" });
    Console.WriteLine($"  Session created: {sessionId}");

    var permissionRequestReceived = false;
    var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            permissionRequestReceived = true;
            Console.WriteLine(isSpanish
                ? $"    [Permission on Resume] Kind: {request.Kind} -> APROBADO"
                : $"    [Permission on Resume] Kind: {request.Kind} -> APPROVED");
            return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
        }
    });

    await session2.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Run 'echo resumed-with-permissions'"
    });
    Console.WriteLine($"  Permission handler triggered on resume: {permissionRequestReceived}");
    await session2.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();

// Clean up temp directory / Limpiar directorio temporal
try { Directory.Delete(workDir, true); } catch { /* ignore */ }

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para modo interactivo (aprobar/denegar cada solicitud)."
    : "  Press Enter for interactive mode (approve/deny each permission request).");
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
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine($"\n    [Permission Request] Kind: {request.Kind}, ToolCallId: {request.ToolCallId}");
        Console.Write(isSpanish ? "    ¿Aprobar? (s/n): " : "    Approve? (y/n): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        var kind = (response == "n" || response == "no")
            ? "denied-interactively-by-user"
            : "approved";
        Console.WriteLine($"    → {kind}");
        return Task.FromResult(new PermissionRequestResult { Kind = kind });
    }
});

Console.WriteLine("  Type messages. You'll approve/deny permission requests interactively.\n");
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
