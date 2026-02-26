using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

namespace ClientDemo;

/// <summary>
/// Orchestrates the Client Lifecycle demo steps.
/// All console I/O is delegated to <see cref="ConsoleRenderer"/>.
/// </summary>
public sealed class ClientDemoRunner(ConsoleRenderer ui, ILogger<CopilotClient> logger)
{
    // ── Public entry point ──────────────────────────────────────────────
    public async Task RunAsync()
    {
        ui.SelectLanguage();
        ui.ShowDemoTitle(
            "01 - CLIENT DEMO: Client Lifecycle & Connection",
            "01 - DEMO: Ciclo de vida y conexion del cliente");

        await RunGuidedDemoAsync();
        await RunInteractiveModeAsync();
    }

    // ── Guided demo ─────────────────────────────────────────────────────
    private async Task RunGuidedDemoAsync()
    {
        // 1. Create
        ui.ShowStep(1,
            "Creating CopilotClient (UseLoggedInUser = true)",
            "Creando CopilotClient (UseLoggedInUser = true)");
        ui.BlankLine();

        var client = CreateClient();
        ui.ShowProperty("Initial state:", "Estado inicial:", client.State.ToString());
        ui.BlankLine();

        // 2. Start
        ui.ShowStep(2, "Starting client (StartAsync)", "Iniciando cliente (StartAsync)");
        await client.StartAsync();
        ui.ShowProperty("State after start:", "Estado tras iniciar:", client.State.ToString());
        ui.BlankLine();

        // 3. Ping
        ui.ShowStep(3, "Ping", "Ping");
        var pong = await client.PingAsync("hello from demo!");
        ui.ShowProperty("Sent:", "Enviado:", "\"hello from demo!\"");
        ui.ShowProperty("Reply:", "Respuesta:", $"\"{pong.Message}\"");
        ui.ShowProperty("Timestamp:", "Timestamp:", pong.Timestamp.ToString() ?? "");
        ui.BlankLine();

        // 4. Status
        ui.ShowStep(4, "Status (GetStatusAsync)", "Estado (GetStatusAsync)");
        var status = await client.GetStatusAsync();
        ui.ShowProperty("Version:", "Version:", status.Version);
        ui.ShowProperty("Protocol Version:", "Version Protocolo:", status.ProtocolVersion.ToString());
        ui.BlankLine();

        // 5. Auth status
        ui.ShowStep(5, "Auth Status (GetAuthStatusAsync)", "Estado de autenticacion (GetAuthStatusAsync)");
        var auth = await client.GetAuthStatusAsync();
        ui.ShowProperty("Authenticated:", "Autenticado:", auth.IsAuthenticated.ToString());
        ui.ShowProperty("Auth Type:", "Tipo:", auth.AuthType ?? "");
        ui.ShowProperty("Message:", "Mensaje:", auth.StatusMessage ?? "");
        ui.BlankLine();

        // 6. List models
        ui.ShowStep(6,
            "List Models (ListModelsAsync)",
            "Listar modelos disponibles (ListModelsAsync)");

        if (auth.IsAuthenticated)
        {
            var models = await client.ListModelsAsync();
            ui.ShowInfo(
                $"Found {models.Count} model(s):",
                $"Se encontraron {models.Count} modelo(s):");
            ui.BlankLine();
            ui.ShowModelsHeader();
            foreach (var m in models)
                ui.ShowModelRow(m.Id, m.Name, m.Capabilities?.ToString() ?? "");
        }
        else
        {
            ui.ShowInfo("Skipped (not authenticated)", "Omitido (no autenticado)");
        }
        ui.BlankLine();

        // 7. Graceful stop
        ui.ShowStep(7, "Graceful Stop (StopAsync)", "Parada ordenada (StopAsync)");
        await client.StopAsync();
        ui.ShowProperty("State after stop:", "Estado tras parar:", client.State.ToString());
        ui.BlankLine();

        // 8. ForceStop demo
        ui.ShowStep(8, "ForceStop Demo", "Demostracion de ForceStop");
        ui.ShowInfo(
            "Starting a new client to demonstrate ForceStop...",
            "Iniciando un nuevo cliente para demostrar ForceStop...");

        var client2 = CreateClient();
        await client2.StartAsync();
        ui.ShowProperty("State:", "State:", client2.State.ToString());

        await client2.ForceStopAsync();
        ui.ShowProperty("State after ForceStop:", "State after ForceStop:", client2.State.ToString());
        await client2.DisposeAsync();
        ui.BlankLine();

        // 9. Cleanup
        await client.DisposeAsync();
    }

    // ── Interactive mode ────────────────────────────────────────────────
    private async Task RunInteractiveModeAsync()
    {
        ui.WaitForEnterOrExit();
        ui.ShowInteractiveHelp();

        var client = CreateClient();
        await client.StartAsync();

        while (true)
        {
            ui.ShowInteractivePrompt();
            var cmd = ui.ReadLine()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(cmd) || cmd is "quit" or "exit") break;

            try
            {
                switch (cmd)
                {
                    case "ping":
                        var msg = ui.Prompt("    Message: ") ?? "test";
                        var p = await client.PingAsync(msg);
                        ui.ShowInteractiveResult(p.Message);
                        break;

                    case "status":
                        var s = await client.GetStatusAsync();
                        ui.ShowInteractiveResult($"Version: {s.Version}, Protocol: {s.ProtocolVersion}");
                        break;

                    case "auth":
                        var a = await client.GetAuthStatusAsync();
                        ui.ShowInteractiveResult($"Authenticated: {a.IsAuthenticated}, Type: {a.AuthType}");
                        break;

                    case "models":
                        var ml = await client.ListModelsAsync();
                        foreach (var m in ml)
                            ui.ShowInteractiveResult($"{m.Id} — {m.Name}");
                        break;

                    default:
                        ui.ShowUnknownCommand();
                        break;
                }
            }
            catch (Exception ex)
            {
                ui.ShowError(ex.Message);
            }
        }

        await client.StopAsync();
        await client.DisposeAsync();
        ui.ShowDone();
    }

    // ── Helpers ─────────────────────────────────────────────────────────
    private CopilotClient CreateClient() => new(new CopilotClientOptions
    {
        UseLoggedInUser = true,
        Logger = logger
    });
}
