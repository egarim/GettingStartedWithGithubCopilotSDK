# 04 – Pre/Post Tool-Use Hooks

## Concept / Concepto

**Hooks** let you intercept tool calls at two points in the lifecycle:

- **OnPreToolUse** — Called *before* a tool executes. You can inspect the tool name/arguments, log the call, and decide whether to **allow** or **deny** execution.
- **OnPostToolUse** — Called *after* a tool executes. You can inspect the result, log it, or modify behavior.

Hooks are configured via `SessionHooks` on the `SessionConfig`. They are ideal for auditing, access control, and debugging.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **PreToolUse — Allow** | Intercept tool calls, log the tool name, and return `PermissionDecision = "allow"`. |
| 2 | **PostToolUse** | Inspect the tool's result after execution. Access `input.ToolResult` and `input.ToolName`. |
| 3 | **Both Hooks Together** | See the full lifecycle: Pre → Tool Executes → Post for each tool call. |
| 4 | **Deny Tool Execution** | Return `PermissionDecision = "deny"` to block a tool call entirely. The model receives a denial and must respond without the tool. |

---

## Key APIs

```csharp
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [myTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            Console.WriteLine($"Tool: {input.ToolName}");
            Console.WriteLine($"Session: {invocation.SessionId}");
            // Return "allow" or "deny"
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
        },
        OnPostToolUse = (input, invocation) =>
        {
            Console.WriteLine($"Tool: {input.ToolName}, Result: {input.ToolResult}");
            return Task.FromResult<PostToolUseHookOutput?>(null);
        }
    }
});
```

---

## Hook Lifecycle

```
  Model requests tool call
       │
       ▼
  ┌─── OnPreToolUse ───┐
  │  Inspect tool name  │
  │  Allow or Deny?     │
  └──────┬──────────────┘
         │
    ┌────┴────┐
    │ "allow" │    │ "deny" │
    ▼         │    ▼        │
  Tool runs   │  Tool skipped
  Result      │  Model told "denied"
    │         │         │
    ▼         │         │
  OnPostToolUse         │
  (inspect result)      │
    │                   │
    ▼                   ▼
  Model gets result   Model responds without it
```

---

## Hooks vs Permissions

| Feature | Hooks (`SessionHooks`) | Permissions (`OnPermissionRequest`) |
|---------|----------------------|-------------------------------------|
| Trigger | Every tool call | Only for destructive operations (write, run) |
| Purpose | Auditing, access control, debugging | User consent for risky actions |
| Granularity | Per tool call | Per permission kind |
| Can deny? | Yes (`PermissionDecision = "deny"`) | Yes (`Kind = "denied-interactively-by-user"`) |

---

## Interactive Mode

Press **Enter** for interactive mode where you are prompted to **allow or deny** each tool call in real time. This simulates a human-in-the-loop approval workflow.

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/04.HooksDemo
```
