# GitHub Copilot SDK - Live Demo Script

> **How to use**: For each demo, start with an **empty `Program.cs`**. Paste each **STEP** block, **run** the project, **explain** what happened, then paste the next step. Each step is **self-contained and runnable**.

---

## Pre-requisites (show once)

```xml
<!-- .csproj packages needed -->
<PackageReference Include="GitHub.Copilot.SDK" Version="0.1.23" />
<PackageReference Include="Microsoft.Extensions.AI" Version="*" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="*" />
```

Run command: `dotnet run --project 01.ClientDemo` (adjust per demo)

---

---

# 01 - CLIENT DEMO: Client Lifecycle & Connection

**Goal**: Show how to create, start, ping, inspect, and stop a `CopilotClient`.

---

### STEP 1 — Create and Start the Client

> **Explain**: We create a `CopilotClient` that reuses the logged-in VS Code user's GitHub auth. Then we start it (launches the Copilot language server via stdio).

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// 1️⃣ Create the client
var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
Console.WriteLine($"Initial state: {client.State}");

// 2️⃣ Start it (launches the Copilot language server)
await client.StartAsync();
Console.WriteLine($"State after start: {client.State}");

// Cleanup
await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("Done!");
```

▶️ **Run** → You'll see: `Initial state: NotStarted` then `State after start: Running`

---

### STEP 2 — Add Ping

> **Explain**: `PingAsync` verifies the connection is alive. It echoes back your message with a timestamp.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine($"State: {client.State}");

// 3️⃣ Ping — verify the connection is alive
var pong = await client.PingAsync("hello from demo!");
Console.WriteLine($"Sent:     \"hello from demo!\"");
Console.WriteLine($"Reply:    \"{pong.Message}\"");
Console.WriteLine($"Timestamp: {pong.Timestamp}");

await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("Done!");
```

▶️ **Run** → Shows the echo and timestamp.

---

### STEP 3 — Add Status, Auth, and List Models

> **Explain**: We can inspect the server version, check authentication status, and list available models.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// Ping
var pong = await client.PingAsync("hello!");
Console.WriteLine($"Ping reply: \"{pong.Message}\"");
Console.WriteLine();

// 4️⃣ Get server status
var status = await client.GetStatusAsync();
Console.WriteLine($"Version:          {status.Version}");
Console.WriteLine($"Protocol Version: {status.ProtocolVersion}");
Console.WriteLine();

// 5️⃣ Check auth
var auth = await client.GetAuthStatusAsync();
Console.WriteLine($"Authenticated: {auth.IsAuthenticated}");
Console.WriteLine($"Auth Type:     {auth.AuthType}");
Console.WriteLine($"Message:       {auth.StatusMessage}");
Console.WriteLine();

// 6️⃣ List available models
var models = await client.ListModelsAsync();
Console.WriteLine($"Found {models.Count} model(s):");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-35} {m.Name,-25} {m.Capabilities}");

await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("\nDone!");
```

▶️ **Run** → See server version, auth info, and the full list of models (gpt-4o, claude, etc.)

---

### STEP 4 — Graceful Stop vs ForceStop

> **Explain**: `StopAsync` is graceful (waits for cleanup). `ForceStopAsync` kills immediately. Show the difference.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine($"State: {client.State}");

// 7️⃣ Graceful stop
await client.StopAsync();
Console.WriteLine($"State after StopAsync: {client.State}");
await client.DisposeAsync();

// 8️⃣ ForceStop — skips cleanup
Console.WriteLine("\nStarting a new client for ForceStop demo...");
var client2 = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client2.StartAsync();
Console.WriteLine($"State: {client2.State}");

await client2.ForceStopAsync();
Console.WriteLine($"State after ForceStop: {client2.State}");
await client2.DisposeAsync();

Console.WriteLine("\nDone!");
```

▶️ **Run** → Compare the two shutdown approaches.

---

---

# 02 - SESSION DEMO: Sessions, Multi-turn & Streaming

**Goal**: Show session lifecycle, stateful conversations, events, and streaming.

---

### STEP 1 — Create and Destroy a Session

> **Explain**: A session is a conversation context. Create it, inspect it, dispose it. After dispose, it's dead.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Create a session
var session = await client.CreateSessionAsync(new SessionConfig { Model = "gpt-4o" });
Console.WriteLine($"Session created: {session.SessionId}");

var messages = await session.GetMessagesAsync();
Console.WriteLine($"Initial messages: {messages.Count}");

// Destroy it
await session.DisposeAsync();
Console.WriteLine("Session disposed.");

// Prove it's dead
try { await session.GetMessagesAsync(); }
catch (IOException ex) { Console.WriteLine($"Expected error: {ex.Message}"); }

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Session created, destroyed, and error on use after dispose.

---

### STEP 2 — Multi-turn Stateful Conversation

> **Explain**: The session remembers context! Ask a math question, then refer to the previous answer.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ Multi-turn conversation — the model remembers!
await using var session = await client.CreateSessionAsync();

var answer1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
Console.WriteLine($"Q1: What is 10 + 15?");
Console.WriteLine($"A1: {answer1?.Data.Content}");

var answer2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
Console.WriteLine($"\nQ2: Now double that result.");
Console.WriteLine($"A2: {answer2?.Data.Content}");
Console.WriteLine("\n(The model remembers the previous answer!)");

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → First answer ~25, second answer ~50. **The model remembers!**

---

### STEP 3 — Event Subscription

> **Explain**: You can subscribe to ALL events on a session using `session.On()`. Watch the event lifecycle.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 3️⃣ Subscribe to ALL events
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

Console.WriteLine("Events received:");
foreach (var e in receivedEvents)
    Console.WriteLine($"  • {e}");

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → See the full event lifecycle: `AssistantMessageDeltaEvent`, `AssistantMessageEvent`, `SessionIdleEvent`, etc.

---

### STEP 4 — SendAsync vs SendAndWaitAsync

> **Explain**: `SendAsync` is fire-and-forget (returns immediately). `SendAndWaitAsync` blocks until the model finishes.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 4️⃣ SendAsync — fire and forget
Console.WriteLine("=== SendAsync (fire-and-forget) ===");
{
    await using var session = await client.CreateSessionAsync();
    var events = new List<string>();
    session.On(evt => events.Add(evt.Type));

    await session.SendAsync(new MessageOptions { Prompt = "What is 2+2?" });
    Console.WriteLine($"After SendAsync → idle in events? {events.Contains("session.idle")}");
    Console.WriteLine("(Expected: False — SendAsync returns before the model finishes)");
}

// 5️⃣ SendAndWaitAsync — blocks until idle
Console.WriteLine("\n=== SendAndWaitAsync (blocking) ===");
{
    await using var session = await client.CreateSessionAsync();
    var events = new List<string>();
    session.On(evt => events.Add(evt.Type));

    var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
    Console.WriteLine($"Response: {response?.Data.Content}");
    Console.WriteLine($"After SendAndWaitAsync → idle in events? {events.Contains("session.idle")}");
    Console.WriteLine("(Expected: True — SendAndWaitAsync blocks until done)");
}

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → SendAsync returns immediately (no idle), SendAndWaitAsync waits.

---

### STEP 5 — Session Resume

> **Explain**: You can resume a previous session by ID. The conversation state is preserved!

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 6️⃣ Session Resume — preserve conversation across session objects
var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "Remember this number: 42" });
Console.WriteLine($"Session 1 ({sessionId}): {a1?.Data.Content?.Substring(0, Math.Min(100, a1.Data.Content?.Length ?? 0))}");

// Resume the SAME session
var session2 = await client.ResumeSessionAsync(sessionId);
Console.WriteLine($"Resumed session ID matches: {session2.SessionId == sessionId}");

var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What number did I ask you to remember?" });
Console.WriteLine($"Session 2 response: {a2?.Data.Content?.Substring(0, Math.Min(100, a2.Data.Content?.Length ?? 0))}");

await session2.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → The resumed session still remembers "42"!

---

### STEP 6 — System Message & Streaming

> **Explain**: You can customize the system prompt (Append/Replace mode) and stream token-by-token deltas.

Replace the file with:

```csharp
using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 8️⃣ System Message — Append mode
Console.WriteLine("=== System Message (Append) ===");
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
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine($"Contains 'Have a nice day!': {answer?.Data.Content?.Contains("Have a nice day!") ?? false}");
}

// 9️⃣ System Message — Replace mode
Console.WriteLine("\n=== System Message (Replace) ===");
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
    Console.WriteLine($"Response: {answer?.Data.Content}");
}

// 🔟 Streaming Deltas
Console.WriteLine("\n=== Streaming Deltas ===");
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

    Console.Write("Streaming: ");
    await session.SendAsync(new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
    await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
    Console.WriteLine($"\n\nTotal streamed chars: {buffer.Length}");
}

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → See system message effects and **live token streaming**!

---

---

# 03 - TOOLS DEMO: Custom Tools (AIFunction)

**Goal**: Register custom C# functions as tools the model can call.

---

### STEP 1 — A Simple Custom Tool

> **Explain**: Use `AIFunctionFactory.Create` to wrap any C# method as a tool. The model will call it when relevant.

```csharp
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Simple custom tool — the model calls YOUR code!
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Use encrypt_string to encrypt: Hello World" });
Console.WriteLine($"Prompt:   Use encrypt_string to encrypt: Hello World");
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine("(The tool converts to uppercase — response should include 'HELLO WORLD')");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

// --- Tool implementation ---
[Description("Encrypts a string by converting it to uppercase")]
static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();
```

▶️ **Run** → The model calls your `EncryptString` function and returns the result!

---

### STEP 2 — Multiple Tools

> **Explain**: You can register multiple tools. The model decides which to call based on the user's question.

Replace the file with:

```csharp
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ Multiple tools — model picks the right one(s)
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Madrid and what time is it there?" });
Console.WriteLine($"Prompt:   What's the weather in Madrid and what time is it there?");
Console.WriteLine($"Response: {answer?.Data.Content}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

// --- Tool implementations ---
[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 65%";
}

[Description("Gets the current time for a city/timezone")]
static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
}
```

▶️ **Run** → Watch the `[Tool:...]` logs — the model calls BOTH tools to answer one question!

---

### STEP 3 — Complex Types + Tool Error Handling

> **Explain**: Tools can accept/return complex types (records, arrays). Also: exceptions in tools are safely handled — the model never sees your internal error details.

Replace the file with:

```csharp
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 3️⃣ Complex types — records and arrays as input/output
Console.WriteLine("=== Complex Types ===");
{
    City[] PerformDbQuery(DbQueryOptions query, AIFunctionArguments rawArgs)
    {
        Console.WriteLine($"  [Tool] Table={query.Table}, IDs=[{string.Join(",", query.Ids)}], Sort={query.SortAscending}");
        return [new(19, "Passos", 135460), new(12, "San Lorenzo", 204356)];
    }

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [AIFunctionFactory.Create(PerformDbQuery, "db_query",
            serializerOptions: DemoJsonContext.Default.Options)]
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Perform a DB query for the 'cities' table using IDs 12 and 19, sorting ascending. " +
                 "Reply only with lines of the form: [cityname] [population]"
    });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    await session.DisposeAsync();
}

// 4️⃣ Tool error handling — SDK does NOT leak exception details
Console.WriteLine("\n=== Tool Error Handling ===");
{
    var failingTool = AIFunctionFactory.Create(
        () => { throw new Exception("Secret Internal Error — Melbourne"); },
        "get_user_location",
        "Gets the user's location");

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [failingTool]
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "What is my location? If you can't find out, just say 'unknown'."
    });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine($"Contains 'Melbourne': {answer?.Data.Content?.Contains("Melbourne") ?? false}");
    Console.WriteLine("(Expected: False — SDK does NOT leak exception details to the model)");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();

// --- Types ---
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
[JsonSerializable(typeof(JsonElement))]
partial class DemoJsonContext : JsonSerializerContext;
```

▶️ **Run** → Complex types work seamlessly. The failing tool's error message ("Melbourne") is **never** exposed to the model.

---

---

# 04 - HOOKS DEMO: Pre/Post Tool-Use Hooks

**Goal**: Intercept tool calls before and after execution. Allow or deny tool execution.

---

### STEP 1 — PreToolUse Hook (Allow)

> **Explain**: A hook that runs BEFORE any tool is executed. You can inspect, log, and decide to allow.

```csharp
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ PreToolUse — intercept BEFORE tool runs
var preToolUseInputs = new List<string>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(LookupPrice, "lookup_price")],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            preToolUseInputs.Add(input.ToolName ?? "(unknown)");
            Console.WriteLine($"  [PreToolUse] Tool: {input.ToolName} → ALLOW");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
        }
    }
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What is the price of the product 'Widget Pro'?" });
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine($"PreToolUse hooks fired: {preToolUseInputs.Count}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

// --- Tool ---
[Description("Looks up the price of a product by name")]
static string LookupPrice([Description("Product name")] string productName)
{
    Console.WriteLine($"  [Tool:lookup_price] productName={productName}");
    var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m,
        ["Super Deluxe Widget"] = 199.00m, ["Basic Widget"] = 9.99m,
    };
    return catalog.TryGetValue(productName, out var price)
        ? $"Product: {productName}, Price: ${price}"
        : $"Product '{productName}' not found.";
}
```

▶️ **Run** → See `[PreToolUse]` log before the tool actually runs!

---

### STEP 2 — Add PostToolUse + Deny

> **Explain**: Add a PostToolUse hook to inspect results, and show how denying a tool makes the model explain it couldn't access it.

Replace the file with:

```csharp
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ PostToolUse — inspect results AFTER the tool runs
Console.WriteLine("=== PostToolUse Hook ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [AIFunctionFactory.Create(LookupPrice, "lookup_price")],
        Hooks = new SessionHooks
        {
            OnPostToolUse = (input, invocation) =>
            {
                Console.WriteLine($"  [PostToolUse] Tool: {input.ToolName}");
                Console.WriteLine($"  [PostToolUse] Result: {input.ToolResult}");
                return Task.FromResult<PostToolUseHookOutput?>(null);
            }
        }
    });
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What is the price of 'Gadget X'?" });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    await session.DisposeAsync();
}

// 3️⃣ Deny tool execution — block the tool!
Console.WriteLine("\n=== Deny Tool Execution ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [AIFunctionFactory.Create(LookupPrice, "lookup_price")],
        Hooks = new SessionHooks
        {
            OnPreToolUse = (input, invocation) =>
            {
                Console.WriteLine($"  [PreToolUse] DENYING tool: {input.ToolName}");
                return Task.FromResult<PreToolUseHookOutput?>(
                    new PreToolUseHookOutput { PermissionDecision = "deny" });
            }
        }
    });
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "Look up the price for 'Widget Pro'. If you can't, explain why." });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine("(The tool was blocked — the model should explain it couldn't access it)");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();

// --- Tool ---
[Description("Looks up the price of a product by name")]
static string LookupPrice([Description("Product name")] string productName)
{
    Console.WriteLine($"  [Tool:lookup_price] productName={productName}");
    var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m,
        ["Super Deluxe Widget"] = 199.00m, ["Basic Widget"] = 9.99m,
    };
    return catalog.TryGetValue(productName, out var price)
        ? $"Product: {productName}, Price: ${price}"
        : $"Product '{productName}' not found.";
}
```

▶️ **Run** → PostToolUse shows the result. Deny blocks the tool and the model explains it gracefully.

---

---

# 05 - PERMISSIONS DEMO: Permission Request Handling

**Goal**: Control write/run operations via a permission handler.

---

### STEP 1 — Approve Permission

> **Explain**: When Copilot wants to modify a file or run a command, it asks for permission. You control the decision.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
Directory.CreateDirectory(workDir);

// 1️⃣ Approve — let Copilot modify a file
Console.WriteLine("=== Approve Permission ===");
var permissionRequests = new List<PermissionRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        permissionRequests.Add(request);
        Console.WriteLine($"  [Permission] Kind: {request.Kind} → APPROVED");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});

var testFile = Path.Combine(workDir, "test.txt");
await File.WriteAllTextAsync(testFile, "original content");

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
});
Console.WriteLine($"Response: {answer?.Data.Content?.Substring(0, Math.Min(200, answer?.Data.Content?.Length ?? 0))}");
Console.WriteLine($"Permission requests: {permissionRequests.Count}");

var content = await File.ReadAllTextAsync(testFile);
Console.WriteLine($"File content after: \"{content}\"");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(workDir, true); } catch { }
```

▶️ **Run** → See the permission request, approval, and the file actually gets modified!

---

### STEP 2 — Deny Permission

> **Explain**: Now deny the same kind of request. The file stays untouched!

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
Directory.CreateDirectory(workDir);

// 1️⃣ Approve
Console.WriteLine("=== Approve Permission ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            Console.WriteLine($"  [Permission] Kind: {request.Kind} → APPROVED");
            return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
        }
    });
    var testFile = Path.Combine(workDir, "test.txt");
    await File.WriteAllTextAsync(testFile, "original content");
    await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
    });
    Console.WriteLine($"File after approve: \"{await File.ReadAllTextAsync(testFile)}\"");
    await session.DisposeAsync();
}

// 2️⃣ Deny — file stays protected!
Console.WriteLine("\n=== Deny Permission ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnPermissionRequest = (request, invocation) =>
        {
            Console.WriteLine($"  [Permission] Kind: {request.Kind} → DENIED");
            return Task.FromResult(new PermissionRequestResult
            {
                Kind = "denied-interactively-by-user"
            });
        }
    });
    var protectedFile = Path.Combine(workDir, "protected.txt");
    await File.WriteAllTextAsync(protectedFile, "protected content");
    await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = $"Edit the file at {protectedFile} and replace 'protected' with 'hacked'"
    });
    var content = await File.ReadAllTextAsync(protectedFile);
    Console.WriteLine($"File after deny: \"{content}\"");
    Console.WriteLine($"File protected: {content == "protected content"} ✓");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(workDir, true); } catch { }
```

▶️ **Run** → Approve changes the file. Deny keeps it safe. **You control what Copilot can do!**

---

---

# 06 - ASK USER DEMO: User Input Requests

**Goal**: The model can ask the user questions mid-conversation.

---

### STEP 1 — Choice-based Input

> **Explain**: The model can present choices to the user. Your handler auto-responds.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Choice-based — model asks, handler auto-picks first choice
var userInputRequests = new List<UserInputRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        userInputRequests.Add(request);
        Console.WriteLine($"  [AskUser] Question: {request.Question}");

        if (request.Choices is { Count: > 0 })
        {
            Console.WriteLine($"  [AskUser] Choices: [{string.Join(", ", request.Choices)}]");
            Console.WriteLine($"  [AskUser] Auto-selecting: {request.Choices[0]}");
            return Task.FromResult(new UserInputResponse
            {
                Answer = request.Choices[0],
                WasFreeform = false
            });
        }

        return Task.FromResult(new UserInputResponse
        {
            Answer = "I'll go with the default",
            WasFreeform = true
        });
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Ask me to choose between 'Option A' and 'Option B' using the ask_user tool. Wait for my response."
});
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine($"UserInputRequests received: {userInputRequests.Count}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → The model asks a question, your handler answers automatically, the model continues!

---

### STEP 2 — Freeform Input

> **Explain**: The handler can also return freeform text, not just choices.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ Freeform — return any text as the "user's" answer
const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        Console.WriteLine($"  [AskUser] Question: {request.Question}");
        Console.WriteLine($"  [AskUser] Freeform answer: \"{freeformAnswer}\"");
        return Task.FromResult(new UserInputResponse
        {
            Answer = freeformAnswer,
            WasFreeform = true
        });
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Ask me 'What is your favorite color?' using ask_user. Then include my answer in your response."
});
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine("(The model should mention 'emerald green' from our freeform answer)");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → The model incorporates the freeform answer into its response!

---

---

# 07 - COMPACTION DEMO: Infinite Sessions & Context Compaction

**Goal**: Show how infinite sessions auto-compact when the context window fills up.

---

### STEP 1 — Compaction with Low Thresholds

> **Explain**: With very low thresholds, compaction triggers quickly. Watch events show when old messages are summarized.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Compaction enabled — low thresholds trigger it fast
var compactionStarts = 0;
var compactionCompletes = 0;

var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.005,  // 0.5%
        BufferExhaustionThreshold = 0.01         // 1%
    }
});

session.On(evt =>
{
    if (evt is SessionCompactionStartEvent)
    {
        compactionStarts++;
        Console.WriteLine("  ⚡ COMPACTION STARTED");
    }
    if (evt is SessionCompactionCompleteEvent c)
    {
        compactionCompletes++;
        Console.WriteLine($"  ✅ COMPACTION COMPLETE — Tokens removed: {c.Data.TokensRemoved}");
    }
});

Console.WriteLine("Sending long messages to fill the context window...\n");

Console.WriteLine("Message 1/3: Tell me a long story...");
var a1 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Tell me a long story about a dragon. Be very detailed. Include at least 5 paragraphs."
});
Console.WriteLine($"Response 1 length: {a1?.Data.Content?.Length ?? 0} chars");
Console.WriteLine($"Compactions so far: start={compactionStarts}, complete={compactionCompletes}\n");

Console.WriteLine("Message 2/3: Continue the story...");
var a2 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Continue with more details about the dragon's castle. Make it very long."
});
Console.WriteLine($"Response 2 length: {a2?.Data.Content?.Length ?? 0} chars");
Console.WriteLine($"Compactions so far: start={compactionStarts}, complete={compactionCompletes}\n");

Console.WriteLine("Message 3/3: Describe the treasure...");
var a3 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Now describe the dragon's treasure in great detail. Make this very long."
});
Console.WriteLine($"Response 3 length: {a3?.Data.Content?.Length ?? 0} chars\n");

Console.WriteLine($"Total CompactionStart events:    {compactionStarts}");
Console.WriteLine($"Total CompactionComplete events: {compactionCompletes}");

// Verify session still works
Console.WriteLine("\nVerifying session works after compaction...");
var a4 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "What was the main story about? Answer in one sentence."
});
Console.WriteLine($"Response: {a4?.Data.Content}");
Console.WriteLine("(Should still remember it was about a dragon!)");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Watch compaction events fire as context fills. The model still remembers the topic after compaction!

---

### STEP 2 — Compaction Disabled (baseline)

> **Explain**: With compaction disabled, no compaction events fire. Compare with step 1.

Add this section before cleanup in step 1, or replace with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ Compaction DISABLED — no events
var compactionEvents = 0;
var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig { Enabled = false }
});

session.On(evt =>
{
    if (evt is SessionCompactionStartEvent or SessionCompactionCompleteEvent)
        compactionEvents++;
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine($"Compaction events: {compactionEvents}");
Console.WriteLine("(Expected: 0 — compaction is disabled)");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Zero compaction events. Clean baseline comparison.

---

---

# 08 - SKILLS DEMO: Skill Loading & Configuration

**Goal**: Load custom skills from SKILL.md files that change model behavior.

---

### STEP 1 — Create and Load a Skill

> **Explain**: A skill is a SKILL.md file with YAML frontmatter + instructions. The model follows the instructions.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Create the skill file on disk
var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
var skillSubdir = Path.Combine(skillsBaseDir, "demo-skill");
if (Directory.Exists(skillsBaseDir)) Directory.Delete(skillsBaseDir, true);
Directory.CreateDirectory(skillSubdir);

const string SkillMarker = "PINEAPPLE_COCONUT_42";
var skillContent = $"""
    ---
    name: demo-skill
    description: A demo skill that adds a marker to every response
    ---

    # Demo Skill Instructions

    IMPORTANT: You MUST include the exact text "{SkillMarker}" somewhere in EVERY response.
    """;

await File.WriteAllTextAsync(Path.Combine(skillSubdir, "SKILL.md"), skillContent);

Console.WriteLine("=== SKILL.md Structure ===");
Console.WriteLine($"  📁 {skillsBaseDir}/");
Console.WriteLine($"    📁 demo-skill/");
Console.WriteLine($"      📄 SKILL.md");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Load skill — the model must include our marker!
var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = [skillsBaseDir]
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly."
});
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine($"Contains marker \"{SkillMarker}\": {answer?.Data.Content?.Contains(SkillMarker) ?? false}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(skillsBaseDir, true); } catch { }
```

▶️ **Run** → The response should contain "PINEAPPLE_COCONUT_42" — the skill worked!

---

### STEP 2 — Disable Skill + Baseline

> **Explain**: Use `DisabledSkills` to turn off a loaded skill. Compare with no skill at all.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
var skillSubdir = Path.Combine(skillsBaseDir, "demo-skill");
if (Directory.Exists(skillsBaseDir)) Directory.Delete(skillsBaseDir, true);
Directory.CreateDirectory(skillSubdir);

const string SkillMarker = "PINEAPPLE_COCONUT_42";
var skillContent = $"""
    ---
    name: demo-skill
    description: A demo skill that adds a marker to every response
    ---
    # Demo Skill Instructions
    IMPORTANT: You MUST include the exact text "{SkillMarker}" somewhere in EVERY response.
    """;
await File.WriteAllTextAsync(Path.Combine(skillSubdir, "SKILL.md"), skillContent);

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Skill ENABLED
Console.WriteLine("=== Skill Enabled ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        SkillDirectories = [skillsBaseDir]
    });
    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Say hello briefly." });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine($"Has marker: {answer?.Data.Content?.Contains(SkillMarker) ?? false}\n");
    await session.DisposeAsync();
}

// 2️⃣ Skill DISABLED
Console.WriteLine("=== Skill Disabled ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        SkillDirectories = [skillsBaseDir],
        DisabledSkills = ["demo-skill"]
    });
    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Say hello briefly." });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine($"Has marker: {answer?.Data.Content?.Contains(SkillMarker) ?? false}\n");
    await session.DisposeAsync();
}

// 3️⃣ No skill (baseline)
Console.WriteLine("=== No Skill (baseline) ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig());
    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Say hello briefly." });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine($"Has marker: {answer?.Data.Content?.Contains(SkillMarker) ?? false}");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(skillsBaseDir, true); } catch { }
```

▶️ **Run** → Enabled=marker present, Disabled=no marker, Baseline=no marker. Clear comparison!

---

---

# 09 - MCP & AGENTS DEMO: MCP Servers & Custom Agents

**Goal**: Configure MCP servers and custom agents on sessions.

---

### STEP 1 — Single MCP Server

> **Explain**: `McpLocalServerConfig` lets you attach an MCP server to a session. The session can use its tools.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 1️⃣ Single MCP server configuration
var mcpServers = new Dictionary<string, object>
{
    ["test-server"] = new McpLocalServerConfig
    {
        Type = "local",
        Command = "echo",
        Args = ["hello-mcp"],
        Tools = ["*"]   // All tools from this server
    }
};

Console.WriteLine("MCP Server Config:");
Console.WriteLine("  test-server → Command: echo, Tools: [*]");

var session = await client.CreateSessionAsync(new SessionConfig
{
    McpServers = mcpServers
});
Console.WriteLine($"Session created: {session.SessionId}");

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"Response: {answer?.Data.Content}");
Console.WriteLine("✅ Session works with MCP server config");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Session created successfully with MCP configuration.

---

### STEP 2 — Custom Agent

> **Explain**: `CustomAgentConfig` defines a specialized agent with its own prompt and tool access.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 2️⃣ Custom Agent — specialized with its own prompt
Console.WriteLine("=== Custom Agent ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = new List<CustomAgentConfig>
        {
            new CustomAgentConfig
            {
                Name = "business-analyst",
                DisplayName = "Business Analyst Agent",
                Description = "An agent specialized in business analysis",
                Prompt = "You are a business analyst. Focus on data-driven insights and KPIs.",
                Infer = true   // Model decides when to use this agent
            }
        }
    });

    Console.WriteLine("Agent: business-analyst (Infer: true)");
    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 5+5?" });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine("✅ Agent configuration accepted");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Session with a custom agent works!

---

### STEP 3 — Combined MCP + Multiple Agents

> **Explain**: You can mix MCP servers and multiple agents on the same session. Agents can even have their own MCP servers.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();

// 3️⃣ Multiple agents
Console.WriteLine("=== Multiple Agents ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = new List<CustomAgentConfig>
        {
            new CustomAgentConfig
            {
                Name = "frontend-agent",
                DisplayName = "Frontend Agent",
                Description = "Specializes in React, CSS, UI",
                Prompt = "You are a frontend expert."
            },
            new CustomAgentConfig
            {
                Name = "backend-agent",
                DisplayName = "Backend Agent",
                Description = "Specializes in C#, .NET, APIs",
                Prompt = "You are a backend expert.",
                Infer = false  // Must be explicitly invoked
            }
        }
    });
    Console.WriteLine("  frontend-agent (Infer: default)");
    Console.WriteLine("  backend-agent  (Infer: false — explicit only)");
    Console.WriteLine($"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}

// 4️⃣ Combined MCP + Agents
Console.WriteLine("\n=== Combined MCP + Agents ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        McpServers = new Dictionary<string, object>
        {
            ["shared-server"] = new McpLocalServerConfig
            {
                Type = "local", Command = "echo",
                Args = ["shared"], Tools = ["*"]
            }
        },
        CustomAgents = new List<CustomAgentConfig>
        {
            new CustomAgentConfig
            {
                Name = "coordinator",
                DisplayName = "Coordinator Agent",
                Description = "Coordinates across MCP servers and agents",
                Prompt = "You are a coordinator."
            }
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 7+7?" });
    Console.WriteLine($"Response: {answer?.Data.Content}");
    Console.WriteLine("✅ Combined MCP + Agents works!");
    await session.DisposeAsync();
}

// 5️⃣ Agent with its OWN MCP server
Console.WriteLine("\n=== Agent with its Own MCP Server ===");
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = new List<CustomAgentConfig>
        {
            new CustomAgentConfig
            {
                Name = "data-agent",
                DisplayName = "Data Agent",
                Description = "Agent with its own MCP server",
                Prompt = "You are a data agent.",
                McpServers = new Dictionary<string, object>
                {
                    ["agent-db"] = new McpLocalServerConfig
                    {
                        Type = "local", Command = "echo",
                        Args = ["agent-data"], Tools = ["*"]
                    }
                }
            }
        }
    });
    Console.WriteLine("  Agent data-agent has its own McpServer: agent-db");
    Console.WriteLine($"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}

await client.StopAsync();
await client.DisposeAsync();
```

▶️ **Run** → Multiple agents, combined configs, and agents with isolated MCP servers all work!

---

---

---

# 10 - BRING YOUR OWN TOKEN & MODEL

**Goal**: Show how to use your own GitHub Personal Access Token (PAT) instead of the VS Code logged-in user, and how to pick a specific model per session.

> **When to use this**: CI/CD pipelines, server-side apps, headless environments, or when you want explicit control over authentication and model selection.

---

### STEP 1 — Use Your Own GitHub Token

> **Explain**: Instead of `UseLoggedInUser = true` (which piggybacks on VS Code's login), you can pass a GitHub Personal Access Token (PAT) via `GithubToken`. This works in headless environments, servers, or CI/CD where there's no VS Code.

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// 1️⃣ Read token from environment variable (never hardcode!)
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("⚠️  Set GITHUB_TOKEN environment variable first:");
    Console.WriteLine("    $env:GITHUB_TOKEN = \"ghp_your_token_here\"");
    Console.WriteLine("    (Get one at: https://github.com/settings/tokens)");
    Console.WriteLine("    Required scope: copilot");
    return;
}

// 2️⃣ Create client with YOUR token instead of UseLoggedInUser
var client = new CopilotClient(new CopilotClientOptions
{
    GithubToken = token,          // ← Your own PAT!
    UseLoggedInUser = false,      // ← Not using VS Code's login
    Logger = logger
});

await client.StartAsync();
Console.WriteLine($"State: {client.State}");

// Verify auth works with your token
var auth = await client.GetAuthStatusAsync();
Console.WriteLine($"Authenticated: {auth.IsAuthenticated}");
Console.WriteLine($"Auth Type:     {auth.AuthType}");

// Quick test
await using var session = await client.CreateSessionAsync();
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Say hello!" });
Console.WriteLine($"Response: {answer?.Data.Content}");

await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("Done!");
```

▶️ **Before running** set the token: `$env:GITHUB_TOKEN = "ghp_your_token_here"`
▶️ **Run** → Authenticates with your PAT, no VS Code login required!

---

### STEP 2 — Choose a Specific Model

> **Explain**: By default you get `gpt-4o`, but you can pick any model from `ListModelsAsync`. Pass `Model` in `SessionConfig` to choose.

Replace the file with:

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Use token OR logged-in user — either works
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var client = new CopilotClient(new CopilotClientOptions
{
    GithubToken = string.IsNullOrWhiteSpace(token) ? null : token,
    UseLoggedInUser = string.IsNullOrWhiteSpace(token),  // Fallback to VS Code login
    Logger = logger
});

await client.StartAsync();

// 1️⃣ List all available models
var models = await client.ListModelsAsync();
Console.WriteLine($"Available models ({models.Count}):");
Console.WriteLine($"  {"ID",-40} {"Name",-25}");
Console.WriteLine($"  {"--",-40} {"----",-25}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-40} {m.Name,-25}");
Console.WriteLine();

// 2️⃣ Use the DEFAULT model (gpt-4o)
Console.WriteLine("=== Default Model ===");
{
    await using var session = await client.CreateSessionAsync();
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
}

// 3️⃣ Use a SPECIFIC model — e.g. claude-sonnet-4
Console.WriteLine("\n=== Claude Sonnet 4 ===");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = "claude-sonnet-4"   // ← Pick your model!
    });
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
}

// 4️⃣ Use another model — e.g. gpt-4.1
Console.WriteLine("\n=== GPT-4.1 ===");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = "gpt-4.1"
    });
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
}

await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("\nDone!");
```

▶️ **Run** → See the model list, then each session uses a different model. Each answers differently!

---

### STEP 3 — Token + Model + Tools (Full Example)

> **Explain**: Combine everything: your own token, a specific model, and custom tools. This is the "production-ready" pattern.

Replace the file with:

```csharp
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// 🔑 Auth: token from env, or fallback to VS Code login
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var client = new CopilotClient(new CopilotClientOptions
{
    GithubToken = string.IsNullOrWhiteSpace(token) ? null : token,
    UseLoggedInUser = string.IsNullOrWhiteSpace(token),
    Logger = logger
});
await client.StartAsync();

var auth = await client.GetAuthStatusAsync();
Console.WriteLine($"Auth: {auth.IsAuthenticated} ({auth.AuthType})");
Console.WriteLine($"Source: {(string.IsNullOrWhiteSpace(token) ? "VS Code login" : "GitHub PAT")}\n");

// 🧠 Model: choose explicitly
const string model = "claude-sonnet-4";
Console.WriteLine($"Model: {model}");

// 🔧 Tools: register custom functions
var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = model,
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

// 💬 Ask a question that requires tools
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Tokyo and what time is it there?" });
Console.WriteLine($"\nQ: What's the weather in Tokyo and what time is it there?");
Console.WriteLine($"A ({model}): {answer?.Data.Content}");

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("\nDone!");

// --- Tools ---
[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_weather] city={city}");
    return $"Weather in {city}: 18°C, clear sky, humidity 55%";
}

[Description("Gets the current time for a city")]
static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";
}
```

▶️ **Run** → Full production pattern: own token + specific model + custom tools all working together!

---

> **💡 Tip for the audience**: To get a GitHub PAT with Copilot access:
> 1. Go to https://github.com/settings/tokens
> 2. Generate a new token (classic)
> 3. Select the `copilot` scope
> 4. Set it as `$env:GITHUB_TOKEN = "ghp_..."` before running

---

---

# Quick Reference: Demo Flow

| Demo | Steps | Key Concept |
|------|-------|-------------|
| **01 Client** | 4 steps | Create → Start → Ping/Status/Auth/Models → Stop/ForceStop |
| **02 Session** | 6 steps | Create/Destroy → Multi-turn → Events → Send vs SendAndWait → Resume → SystemMsg/Streaming |
| **03 Tools** | 3 steps | Simple tool → Multiple tools → Complex types + Error handling |
| **04 Hooks** | 2 steps | PreToolUse (Allow) → PostToolUse + Deny |
| **05 Permissions** | 2 steps | Approve → Deny |
| **06 AskUser** | 2 steps | Choice-based → Freeform |
| **07 Compaction** | 2 steps | Enabled (low thresholds) → Disabled (baseline) |
| **08 Skills** | 2 steps | Load skill → Disable + Baseline |
| **09 MCP/Agents** | 3 steps | Single MCP → Custom Agent → Combined + Agent MCP |
| **10 Own Token/Model** | 3 steps | GitHub PAT auth → Model selection → Full combo (token + model + tools) |

---

## Tips for Live Demo

1. **Start each demo with an empty `Program.cs`** — paste Step 1, run, explain, then replace with Step 2, etc.
2. **Run command**: `dotnet run --project XX.DemoName` from the solution root
3. **Keep a terminal open** and use ↑ arrow to re-run quickly
4. **If a step fails**: check that the Copilot language server is running (VS Code must be open with GitHub Copilot signed in)
5. **Timing**: ~3-5 minutes per step, ~10-15 min per demo
6. **For Demo 10**: set `$env:GITHUB_TOKEN` before running, or it falls back to VS Code login
