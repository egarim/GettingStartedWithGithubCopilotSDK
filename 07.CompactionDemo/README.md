# 07 – Infinite Sessions & Context Compaction

## Concept / Concepto

Every LLM has a finite **context window** (maximum number of tokens). In a long conversation, you eventually run out of space. The SDK solves this with **Infinite Sessions** — when the context window fills up, it automatically **compacts** the conversation by summarizing older messages and removing tokens, while preserving the key information.

This demo shows how to enable infinite sessions, configure compaction thresholds, monitor compaction events, and verify the session remains functional after compaction.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Compaction Enabled** | Configure `InfiniteSessionConfig` with low thresholds to trigger compaction quickly. Monitor `SessionCompactionStartEvent` and `SessionCompactionCompleteEvent`. |
| 2 | **Compaction Disabled** | Set `Enabled = false` to confirm no compaction events are fired. |

---

## Key APIs

```csharp
var session = await client.CreateSessionAsync(new SessionConfig
{
    InfiniteSessions = new InfiniteSessionConfig
    {
        Enabled = true,
        // Percentage of context window usage thresholds
        BackgroundCompactionThreshold = 0.005,  // 0.5% → start background compaction
        BufferExhaustionThreshold = 0.01         // 1% → block and compact before continuing
    }
});

// Monitor compaction events
session.On(evt =>
{
    if (evt is SessionCompactionStartEvent)
        Console.WriteLine("Compaction started!");
    if (evt is SessionCompactionCompleteEvent c)
        Console.WriteLine($"Compaction done. Tokens removed: {c.Data.TokensRemoved}");
});
```

---

## How Compaction Works

```
  Message 1  ─┐
  Message 2   │
  Message 3   │  Context fills up...
  Message 4   │
  ...         │
  Message N  ─┘
       │
       ▼ BackgroundCompactionThreshold reached
  ┌──────────────────────────────────┐
  │  SessionCompactionStartEvent     │
  │  SDK summarizes old messages     │
  │  Removes N tokens                │
  │  SessionCompactionCompleteEvent  │
  │  { Success: true,                │
  │    TokensRemoved: 5000 }         │
  └──────────────────────────────────┘
       │
       ▼ Session continues with summarized context
  Message N+1 works normally
```

---

## Compaction Thresholds

| Threshold | Purpose | Default | Demo Value |
|-----------|---------|---------|------------|
| `BackgroundCompactionThreshold` | When to start compaction **in the background** (as a fraction of context window) | SDK default | `0.005` (0.5%) |
| `BufferExhaustionThreshold` | When to **block** and compact before accepting new messages | SDK default | `0.01` (1%) |

> **Note**: The demo uses very low thresholds to trigger compaction quickly. In production, use higher values (or defaults) for better performance.

---

## Compaction Events

| Event | Data | Description |
|-------|------|-------------|
| `SessionCompactionStartEvent` | — | Compaction has been triggered |
| `SessionCompactionCompleteEvent` | `Success`, `TokensRemoved` | Compaction finished with results |

---

## Interactive Mode

Press **Enter** to start an infinite chat session. Keep sending long messages to fill up the context window and trigger compaction. The prompt shows a compaction counter so you can see when compaction events occur.

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/07.CompactionDemo
```
