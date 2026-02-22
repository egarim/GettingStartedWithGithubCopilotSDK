# 06 – User Input Requests (Ask User)

## Concept / Concepto

The model can **ask the user a question** during a conversation using the built-in `ask_user` tool. When invoked, the SDK triggers the `OnUserInputRequest` handler in your code. The handler receives the question text and optionally a list of choices, then returns the user's answer. This enables interactive, multi-step workflows where the model needs clarification.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Choice-based Input** | The model provides a list of `Choices` (e.g., "Option A", "Option B"). You auto-select or let the user pick. |
| 2 | **Verify Choices** | Inspect `request.Choices` to confirm the model sent the expected options (e.g., "Red", "Blue"). |
| 3 | **Freeform Input** | When no choices are provided, the user types a free-text answer. Set `WasFreeform = true`. |

---

## Key APIs

```csharp
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        // request.Question — the model's question text
        // request.Choices — optional list of choices (can be null or empty)
        // invocation.SessionId — session context

        if (request.Choices is { Count: > 0 })
        {
            // Choice-based: pick one
            return Task.FromResult(new UserInputResponse
            {
                Answer = request.Choices[0],
                WasFreeform = false
            });
        }

        // Freeform: user types their own answer
        return Task.FromResult(new UserInputResponse
        {
            Answer = "My custom answer",
            WasFreeform = true
        });
    }
});
```

---

## How Ask User Works

```
  You send a prompt
       │
       ▼
  Model processes...
  Model needs user input
       │
       ▼
  ┌─── OnUserInputRequest ──────────────┐
  │  Question: "What color do you want?" │
  │  Choices: ["Red", "Blue", "Green"]   │
  │  (or no choices → freeform)          │
  └──────────┬───────────────────────────┘
             │
  Your handler returns UserInputResponse
  { Answer: "Blue", WasFreeform: false }
             │
             ▼
  Model incorporates the answer
  Model sends final response
```

---

## UserInputRequest Properties

| Property | Type | Description |
|----------|------|-------------|
| `Question` | `string` | The question the model is asking |
| `Choices` | `List<string>?` | Optional list of predefined choices |

## UserInputResponse Properties

| Property | Type | Description |
|----------|------|-------------|
| `Answer` | `string` | The user's answer |
| `WasFreeform` | `bool` | `true` if the user typed freely; `false` if they picked a choice |

---

## Interactive Mode

Press **Enter** to chat interactively. When the model asks a question, you'll see the choices (if any) and type your answer directly in the console. Try prompts like:

- "Ask me what programming language I prefer using ask_user"
- "Ask me to choose between pizza and sushi"

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/06.AskUserDemo
```
