# 05 – Permission Request Handling

## Concept / Concepto

When the Copilot model wants to perform a **destructive or risky action** (write a file, run a command, etc.), the SDK raises a **permission request** via the `OnPermissionRequest` handler. Your code decides whether to approve or deny the operation. This is distinct from hooks — permissions are triggered specifically for actions that could modify the user's environment.

This demo covers approval, denial, async handlers, error recovery, and using permissions on resumed sessions.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Approve Permission** | Return `Kind = "approved"` to allow the model to write/modify files. Verify the file was actually changed. |
| 2 | **Deny Permission** | Return `Kind = "denied-interactively-by-user"` to block the operation. Verify the file remains unchanged. |
| 3 | **Default Behavior** | Without a handler, the session still works for non-destructive operations. Permissions are only requested for write/run actions. |
| 4 | **Async Handler** | Permission handlers can be `async` — simulate a delay (e.g., external approval system) before responding. |
| 5 | **ToolCallId Inspection** | Each permission request includes a `ToolCallId` that correlates with the specific tool invocation. |
| 6 | **Handler Error Recovery** | If your handler throws an exception, the SDK handles it gracefully — permission is denied automatically, session continues. |
| 7 | **Permission on Resume** | `ResumeSessionConfig` also accepts `OnPermissionRequest`, so resumed sessions can have their own permission handler. |

---

## Key APIs

```csharp
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        // request.Kind — type of permission ("write", "run", etc.)
        // request.ToolCallId — correlates to the tool invocation
        // invocation.SessionId — the current session

        return Task.FromResult(new PermissionRequestResult
        {
            Kind = "approved"                            // or:
            // Kind = "denied-interactively-by-user"     // block it
        });
    }
});

// Permission on session resume
var resumed = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
{
    OnPermissionRequest = (request, invocation) =>
        Task.FromResult(new PermissionRequestResult { Kind = "approved" })
});
```

---

## Permission Flow

```
  Model wants to write a file / run a command
       │
       ▼
  ┌─── OnPermissionRequest ───┐
  │  Kind: "write" / "run"    │
  │  ToolCallId: "call_abc"   │
  │  Your decision?           │
  └──────┬────────────────────┘
         │
    ┌────┴────────────┐
    │ "approved"      │  "denied-..."    │  Handler throws
    ▼                 ▼                  ▼
  Action proceeds   Action blocked     SDK catches exception
  File is modified  File unchanged     Permission auto-denied
```

---

## Permission Kinds

| Response Kind | Effect |
|---------------|--------|
| `"approved"` | The model proceeds with the action |
| `"denied-interactively-by-user"` | The model is told the user denied the action |

---

## Interactive Mode

Press **Enter** to interactively approve or deny each permission request as you chat. Try asking the model to create or edit files and see how your decisions affect the outcome.

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/05.PermissionsDemo
```
