# 01 – Client Lifecycle & Connection

## Concept / Concepto

The `CopilotClient` is the entry point to the GitHub Copilot SDK. It manages the underlying process (Stdio transport), authentication, and the ability to query the Copilot service. Before creating any sessions or sending messages, you must start the client and ensure it is connected.

This demo covers the **full client lifecycle**: creation → start → usage → stop → dispose, plus the difference between a graceful shutdown and a forced stop.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Creating a CopilotClient** | Instantiate with `CopilotClientOptions` and `UseLoggedInUser = true` (uses the VS Code / GitHub CLI logged-in user). |
| 2 | **StartAsync** | Starts the underlying Copilot process and establishes the Stdio connection. |
| 3 | **PingAsync** | Sends a ping message and receives a pong — confirms the connection is alive. |
| 4 | **GetStatusAsync** | Retrieves version and protocol version of the running Copilot agent. |
| 5 | **GetAuthStatusAsync** | Checks whether the current user is authenticated, and the auth type. |
| 6 | **ListModelsAsync** | Lists all available models (e.g., `gpt-4o`, `claude-sonnet-4`) with their capabilities. |
| 7 | **StopAsync** (graceful) | Sends a shutdown request and waits for cleanup to complete. |
| 8 | **ForceStopAsync** | Kills the process immediately — skips cleanup. Use only when `StopAsync` hangs. |
| 9 | **DisposeAsync** | Releases all resources. Always call after stop. |

---

## Key APIs

```csharp
// Create the client
var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});

// Lifecycle
await client.StartAsync();           // Connect
var pong  = await client.PingAsync("hello");
var status = await client.GetStatusAsync();
var auth   = await client.GetAuthStatusAsync();
var models = await client.ListModelsAsync();
await client.StopAsync();            // Graceful shutdown
await client.ForceStopAsync();       // Hard kill
await client.DisposeAsync();         // Release resources
```

---

## Client States

The client transitions through these states:

```
Created → Starting → Running → Stopping → Stopped
                                  ↑ ForceStop (immediate)
```

- **Created** — Just instantiated, not connected yet.
- **Running** — Connected and ready to create sessions.
- **Stopped** — Process terminated, resources released.

---

## Interactive Mode

After the automated demos, press **Enter** to enter an interactive command loop where you can type:

- `ping` — Send a custom ping message
- `status` — Check version info
- `auth` — Check authentication status
- `models` — List available models
- `quit` — Exit

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/01.ClientDemo
```

> **Prerequisite**: You must be logged in to GitHub via VS Code or `gh auth login`.
