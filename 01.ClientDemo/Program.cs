using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 01 – CLIENT DEMO: Client Lifecycle & Connection
// 01 – DEMO DE CLIENTE: Ciclo de vida y conexión del cliente
//
// Demonstrates / Demuestra:
//   • Creating a CopilotClient with UseLoggedInUser
//   • Starting and connecting (Stdio transport)
//   • Ping, Status, Auth Status, List Models
//   • Graceful Stop vs ForceStop
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
    Console.WriteLine("  01 - DEMO: Ciclo de vida y conexion del cliente");
}
else
{
    Console.WriteLine("  01 - CLIENT DEMO: Client Lifecycle & Connection");
}
Console.WriteLine("================================================================");
Console.WriteLine();

// -- 1. Create the client / Crear el cliente --
Console.WriteLine(isSpanish
    ? "=== 1. Creando CopilotClient (UseLoggedInUser = true) ==="
    : "=== 1. Creating CopilotClient (UseLoggedInUser = true) ===");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});

Console.WriteLine(isSpanish
    ? $"  Estado inicial: {client.State}"
    : $"  Initial state: {client.State}");
Console.WriteLine();

// -- 2. Start the client / Iniciar el cliente --
Console.WriteLine(isSpanish
    ? "=== 2. Iniciando cliente (StartAsync) ==="
    : "=== 2. Starting client (StartAsync) ===");
await client.StartAsync();
Console.WriteLine(isSpanish
    ? $"  Estado tras iniciar: {client.State}"
    : $"  State after start: {client.State}");
Console.WriteLine();

// -- 3. Ping / Verificacion de conexion --
Console.WriteLine("=== 3. Ping ===");
var pong = await client.PingAsync("hello from demo!");
Console.WriteLine(isSpanish
    ? $"  Enviado:  \"hello from demo!\""
    : $"  Sent:     \"hello from demo!\"");
Console.WriteLine(isSpanish
    ? $"  Respuesta: \"{pong.Message}\""
    : $"  Reply:    \"{pong.Message}\"");
Console.WriteLine($"  Timestamp: {pong.Timestamp}");
Console.WriteLine();

// -- 4. Get Status / Obtener estado --
Console.WriteLine(isSpanish
    ? "=== 4. Estado (GetStatusAsync) ==="
    : "=== 4. Status (GetStatusAsync) ===");
var status = await client.GetStatusAsync();
Console.WriteLine(isSpanish
    ? $"  Version:          {status.Version}"
    : $"  Version:          {status.Version}");
Console.WriteLine(isSpanish
    ? $"  Version Protocolo: {status.ProtocolVersion}"
    : $"  Protocol Version:  {status.ProtocolVersion}");
Console.WriteLine();

// -- 5. Auth Status / Estado de autenticacion --
Console.WriteLine(isSpanish
    ? "=== 5. Estado de autenticacion (GetAuthStatusAsync) ==="
    : "=== 5. Auth Status (GetAuthStatusAsync) ===");
var auth = await client.GetAuthStatusAsync();
Console.WriteLine(isSpanish
    ? $"  Autenticado: {auth.IsAuthenticated}"
    : $"  Authenticated: {auth.IsAuthenticated}");
Console.WriteLine(isSpanish
    ? $"  Tipo:    {auth.AuthType}"
    : $"  Auth Type: {auth.AuthType}");
Console.WriteLine(isSpanish
    ? $"  Mensaje: {auth.StatusMessage}"
    : $"  Message: {auth.StatusMessage}");
Console.WriteLine();

// -- 6. List Models / Listar modelos --
Console.WriteLine(isSpanish
    ? "=== 6. Listar modelos disponibles (ListModelsAsync) ==="
    : "=== 6. List Models (ListModelsAsync) ===");
if (auth.IsAuthenticated)
{
    var models = await client.ListModelsAsync();
    Console.WriteLine(isSpanish
        ? $"  Se encontraron {models.Count} modelo(s):"
        : $"  Found {models.Count} model(s):");
    Console.WriteLine();
    Console.WriteLine($"  {"ID",-35} {"Name",-25} {"Capabilities"}");
    Console.WriteLine($"  {"--",-35} {"----",-25} {"------------"}");
    foreach (var m in models)
    {
        Console.WriteLine($"  {m.Id,-35} {m.Name,-25} {m.Capabilities}");
    }
}
else
{
    Console.WriteLine(isSpanish
        ? "  Omitido (no autenticado)"
        : "  Skipped (not authenticated)");
}
Console.WriteLine();

// -- 7. Graceful Stop / Parada ordenada --
Console.WriteLine(isSpanish
    ? "=== 7. Parada ordenada (StopAsync) ==="
    : "=== 7. Graceful Stop (StopAsync) ===");
await client.StopAsync();
Console.WriteLine(isSpanish
    ? $"  Estado tras parar: {client.State}"
    : $"  State after stop: {client.State}");
Console.WriteLine();

// -- 8. ForceStop demo / Demostracion ForceStop --
Console.WriteLine(isSpanish
    ? "=== 8. Demostracion de ForceStop ==="
    : "=== 8. ForceStop Demo ===");
Console.WriteLine(isSpanish
    ? "  Iniciando un nuevo cliente para demostrar ForceStop..."
    : "  Starting a new client to demonstrate ForceStop...");

var client2 = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client2.StartAsync();
Console.WriteLine($"  State: {client2.State}");

// ForceStop skips cleanup / ForceStop omite la limpieza
await client2.ForceStopAsync();
Console.WriteLine($"  State after ForceStop: {client2.State}");
await client2.DisposeAsync();
Console.WriteLine();

// ── 9. Cleanup / Limpieza ────────────────────────────────────────────
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo --
Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  Presiona Enter para modo interactivo, o Ctrl+C para salir.");
}
else
{
    Console.WriteLine("  Press Enter for interactive mode, or Ctrl+C to exit.");
}
Console.WriteLine("================================================================");
Console.ReadLine();

Console.WriteLine(isSpanish
    ? "  Comandos: ping, status, auth, models, quit"
    : "  Interactive commands: ping, status, auth, models, quit");
Console.WriteLine();

var interactiveClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await interactiveClient.StartAsync();

while (true)
{
    Console.Write("  > ");
    var cmd = Console.ReadLine()?.Trim().ToLowerInvariant();
    if (string.IsNullOrEmpty(cmd) || cmd == "quit" || cmd == "exit") break;

    try
    {
        switch (cmd)
        {
            case "ping":
                Console.Write("    Message: ");
                var msg = Console.ReadLine() ?? "test";
                var p = await interactiveClient.PingAsync(msg);
                Console.WriteLine($"    → {p.Message}");
                break;
            case "status":
                var s = await interactiveClient.GetStatusAsync();
                Console.WriteLine($"    Version: {s.Version}, Protocol: {s.ProtocolVersion}");
                break;
            case "auth":
                var a = await interactiveClient.GetAuthStatusAsync();
                Console.WriteLine($"    Authenticated: {a.IsAuthenticated}, Type: {a.AuthType}");
                break;
            case "models":
                var ml = await interactiveClient.ListModelsAsync();
                foreach (var m in ml) Console.WriteLine($"    {m.Id} — {m.Name}");
                break;
            default:
                Console.WriteLine(isSpanish
                    ? "    Comando desconocido. Prueba: ping, status, auth, models, quit"
                    : "    Unknown command. Try: ping, status, auth, models, quit");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"    Error: {ex.Message}");
    }
}

await interactiveClient.StopAsync();
await interactiveClient.DisposeAsync();
Console.WriteLine(isSpanish ? "\n  ¡Listo!" : "\n  Done!");
