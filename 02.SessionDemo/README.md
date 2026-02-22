# 02 – Session Lifecycle, Events & Multi-turn

## Concept / Concepto

A **session** (`CopilotSession`) represents a single conversation with the Copilot model. Sessions are stateful — the model remembers all previous messages within the same session, enabling multi-turn conversations. Sessions emit events (like streaming deltas, idle notifications, errors) that you can subscribe to.

This demo covers the complete session lifecycle, event-driven programming with the SDK, and advanced features like session resume, system messages, and streaming.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Create & Destroy** | `CreateSessionAsync` with optional `SessionConfig` (model, streaming, etc.), and `DisposeAsync` to release. |
| 2 | **Multi-turn Conversation** | Send multiple messages in the same session — the model retains context. |
| 3 | **Event Subscription** | Use `session.On(evt => ...)` to subscribe to all session events (`SessionIdleEvent`, `AssistantMessageEvent`, etc.). |
| 4 | **SendAsync vs SendAndWaitAsync** | `SendAsync` returns immediately (fire-and-forget). `SendAndWaitAsync` blocks until the model finishes (`SessionIdleEvent`). |
| 5 | **SendAndWaitAsync** | Demonstrates that events are already received when the call returns. |
| 6 | **Session Resume** | Use `ResumeSessionAsync(sessionId)` to reconnect to a previous session and continue the conversation. |
| 7 | **Resume Error Handling** | Attempting to resume a non-existent session throws `IOException`. |
| 8 | **System Message (Append)** | `SystemMessageConfig` with `Mode = Append` adds instructions after the default system prompt. |
| 9 | **System Message (Replace)** | `Mode = Replace` completely overrides the system prompt (e.g., change the assistant's name). |
| 10 | **Streaming Deltas** | Enable `Streaming = true` and listen for `AssistantMessageDeltaEvent` for real-time token-by-token output. |

---

## Key APIs

```csharp
// Create a session
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-4o",
    Streaming = true,
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = "Always end with 'Have a nice day!'"
    }
});

// Send messages
await session.SendAsync(new MessageOptions { Prompt = "Hello" });                    // fire-and-forget
var reply = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Hello" }); // blocks until done

// Subscribe to events
var sub = session.On(evt =>
{
    if (evt is AssistantMessageDeltaEvent delta) Console.Write(delta.Data.DeltaContent);
    if (evt is SessionIdleEvent) Console.WriteLine("Turn complete.");
});
sub.Dispose(); // unsubscribe

// Resume a previous session
var resumed = await client.ResumeSessionAsync(sessionId);

// Get message history
var messages = await session.GetMessagesAsync();
```

---

## Event Types

| Event | When |
|-------|------|
| `AssistantMessageEvent` | Model produces a complete message |
| `AssistantMessageDeltaEvent` | Individual streaming token (when `Streaming = true`) |
| `SessionIdleEvent` | Model's turn is complete |
| `SessionErrorEvent` | An error occurred during the turn |
| `SessionResumeEvent` | Session was resumed via `ResumeSessionAsync` |

---

## System Message Modes

| Mode | Behavior |
|------|----------|
| `Append` | Your content is added **after** the default Copilot system prompt |
| `Replace` | Your content **completely replaces** the default system prompt |

---

## Interactive Mode

Press **Enter** after the automated demos to start an interactive streaming chat. Type messages and see responses streamed token-by-token.

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/02.SessionDemo
```
