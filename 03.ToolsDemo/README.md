# 03 – Custom Tools (AIFunction)

## Concept / Concepto

**Tools** extend the model's capabilities by letting it call your code. When the model decides it needs external data or actions (e.g., look up a price, query a database), it invokes a tool you registered. The SDK uses `Microsoft.Extensions.AI`'s `AIFunctionFactory.Create` to turn regular C# methods into tools the model can call.

This demo shows how to create tools of varying complexity, handle errors safely, and control which built-in tools are available.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Simple Tool** | A single function (`encrypt_string`) registered via `AIFunctionFactory.Create`. The `[Description]` attribute tells the model what it does. |
| 2 | **Multiple Tools** | Register several tools (`get_weather`, `get_time`) on one session — the model decides which to call. |
| 3 | **Complex Types** | Use C# `record` types for input/output (`DbQueryOptions`, `City[]`). Requires a `JsonSerializerContext` for NativeAOT safety. |
| 4 | **Tool Error Handling** | When a tool throws an exception, the SDK catches it and does **NOT** leak the error message to the model — the model only sees a generic failure. |
| 5 | **AvailableTools / ExcludedTools** | Filter which built-in Copilot tools are available in the session. `AvailableTools` is an allowlist; `ExcludedTools` is a denylist. |

---

## Key APIs

```csharp
// Simple tool — just a C# method with [Description]
[Description("Encrypts a string by converting to uppercase")]
static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();

// Register on session
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
});

// Complex types with NativeAOT-safe serialization
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
partial class DemoJsonContext : JsonSerializerContext;

// Register with custom serializer
var tool = AIFunctionFactory.Create(PerformDbQuery, "db_query",
    serializerOptions: DemoJsonContext.Default.Options);

// Access ToolInvocation from inside the tool
City[] PerformDbQuery(DbQueryOptions query, AIFunctionArguments rawArgs)
{
    var invocation = (ToolInvocation)rawArgs.Context![typeof(ToolInvocation)]!;
    // invocation.SessionId, invocation.ToolCallId, etc.
    return [new(1, "Madrid", 3223000)];
}

// Filter built-in tools
new SessionConfig { AvailableTools = ["view", "edit"] }   // allowlist
new SessionConfig { ExcludedTools = ["view"] }             // denylist
```

---

## How Tools Work

```
  You (host)                     Copilot Model
  ──────────                     ────────────
  Register tools on session  →   Model sees tool schemas
  Send prompt                →   Model processes prompt
                             ←   Model calls tool (tool_use event)
  SDK executes your C# code
  SDK returns result         →   Model incorporates result
                             ←   Model sends final response
```

1. You register C# methods as tools via `AIFunctionFactory.Create`.
2. The model receives the tool schemas (name, description, parameters).
3. When the model decides to use a tool, the SDK automatically calls your method.
4. The return value is sent back to the model as the tool result.
5. If your method throws, the SDK returns a generic error — no secrets leaked.

---

## Interactive Mode

Press **Enter** to chat interactively with all three tools (`encrypt_string`, `get_weather`, `get_time`) available. Ask things like:
- "Encrypt the phrase 'hello world'"
- "What's the weather in Tokyo?"
- "What time is it in New York?"

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/03.ToolsDemo
```
