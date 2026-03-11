# Demo 11 — Bring Your Own Key (BYOK)

Use **custom models** from any LLM provider (OpenRouter, Anthropic, Azure OpenAI, Google AI Studio, xAI, etc.) through **GitHub Copilot's BYOK feature** — no extra SDK or direct API calls needed.

Your enterprise or org admin adds the API key in GitHub settings, and the custom models automatically appear in `ListModelsAsync()`. You use them via `SessionConfig.Model` exactly like built-in models.

## How BYOK Works

```
┌──────────────────────────────────────────────────────────────┐
│  GitHub Enterprise / Org Settings                            │
│  AI Controls > Copilot > Custom Models > Add API Key         │
│                                                              │
│  Provider: OpenRouter (or Anthropic, Azure OpenAI, etc.)     │
│  API Key:  sk-or-v1-xxxxxxxx                                 │
│  Models:   openai/gpt-4o-mini, anthropic/claude-3.5-haiku    │
└──────────────────────┬───────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────┐
│  Your Code (this demo)                                       │
│                                                              │
│  var models = await client.ListModelsAsync();                │
│  // ← Custom models appear here automatically!               │
│                                                              │
│  var session = await client.CreateSessionAsync(              │
│      new SessionConfig { Model = "custom-model-id" });       │
└──────────────────────────────────────────────────────────────┘
```

## Supported Providers

| Provider | Type |
|----------|------|
| OpenAI | Direct |
| Anthropic | Direct |
| OpenRouter | OpenAI-compatible |
| Azure OpenAI / Microsoft Foundry | Deployment URL |
| Google AI Studio | Direct |
| AWS Bedrock | Direct |
| xAI | Direct |

Docs: [Using your LLM provider API keys with Copilot](https://docs.github.com/en/copilot/how-tos/administer-copilot/manage-for-enterprise/use-your-own-api-keys)

## Prerequisites

1. Enterprise or org admin adds the API key in GitHub settings
2. Admin enables the custom models for your organization
3. You're logged into GitHub Copilot (VS Code or PAT)

## What This Demo Covers

| # | Section | What It Shows |
|---|---------|---------------|
| 1 | Start Client | Connect with VS Code login or GitHub PAT |
| 2 | List Models | See all models — built-in + custom BYOK |
| 3 | Default Model | Chat with the default model |
| 4 | Specific Model | Pick a model by ID from the list |
| 5 | Compare Models | Same prompt across multiple models |
| 6 | Tools + BYOK | Custom tools work with any model |
| 7 | Streaming | Stream responses from custom models |
| 8 | Interactive | Free-form chat loop |

## Key Pattern

```csharp
// No special setup — custom models just appear!
var models = await client.ListModelsAsync();

// Use any model (built-in or BYOK custom) by ID
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "your-custom-model-id"   // ← from BYOK
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Hello!" });
```

## Run

```bash
dotnet run --project 11.OpenRouterDemo
```
