# 09 – MCP Servers & Custom Agents

## Concept / Concepto

The SDK supports two powerful extensibility mechanisms:

- **MCP (Model Context Protocol) Servers** — External tool servers that follow the MCP standard. You configure them on a session and the model can call tools they expose, expanding capabilities beyond your C# code.
- **Custom Agents** — Named agents with their own system prompt, tool restrictions, and even their own MCP servers. Agents can be invoked explicitly or inferred by the model (`Infer = true`).

Both can be configured on `SessionConfig` (at creation) and on `ResumeSessionConfig` (when resuming a session).

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Single MCP Server** | Configure `McpLocalServerConfig` with `Type`, `Command`, `Args`, and `Tools = ["*"]`. |
| 2 | **Multiple MCP Servers** | Add several MCP servers to one session — each with a unique key. |
| 3 | **Custom Agent** | Define a `CustomAgentConfig` with `Name`, `DisplayName`, `Description`, `Prompt`, and `Infer`. |
| 4 | **Agent with Tools** | Restrict an agent to specific tools via `Tools = ["bash", "edit"]`. |
| 5 | **Agent with MCP Servers** | Give an agent its own isolated MCP server connections. |
| 6 | **Multiple Agents** | Configure several agents on one session. `Infer = true` lets the model choose; `Infer = false` requires explicit invocation. |
| 7 | **Combined MCP + Agents** | Use both MCP servers and custom agents on the same session. |
| 8 | **MCP/Agents on Resume** | Add MCP servers and agents when resuming a session via `ResumeSessionConfig`. |

---

## Key APIs

```csharp
// MCP Server configuration
var session = await client.CreateSessionAsync(new SessionConfig
{
    McpServers = new Dictionary<string, object>
    {
        ["my-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "npx",                    // or any executable
            Args = ["-y", "@my/mcp-server"],
            Tools = ["*"]                        // expose all tools
        }
    }
});

// Custom Agent configuration
var session = await client.CreateSessionAsync(new SessionConfig
{
    CustomAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "business-analyst",
            DisplayName = "Business Analyst Agent",
            Description = "Specialized in business analysis",
            Prompt = "You are a business analyst. Focus on KPIs and data-driven insights.",
            Infer = true,                        // model decides when to use
            Tools = ["bash", "edit"],             // optional: restrict tools
            McpServers = new Dictionary<string, object>  // optional: agent's own MCP
            {
                ["agent-db"] = new McpLocalServerConfig { ... }
            }
        }
    }
});

// On session resume
var resumed = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
{
    McpServers = new Dictionary<string, object> { ... },
    CustomAgents = new List<CustomAgentConfig> { ... }
});
```

---

## MCP Server Configuration

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string` | `"local"` for local process |
| `Command` | `string` | Executable to run (e.g., `npx`, `python`, `dotnet`) |
| `Args` | `string[]` | Command-line arguments |
| `Tools` | `string[]` | Tool filter — `["*"]` for all tools, or specific names |

---

## Custom Agent Configuration

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Unique identifier for the agent |
| `DisplayName` | `string` | Human-readable name |
| `Description` | `string` | What the agent does (shown to the model) |
| `Prompt` | `string` | System prompt — defines the agent's personality and expertise |
| `Infer` | `bool` | If `true`, the model automatically decides when to use this agent |
| `Tools` | `string[]?` | Optional tool restrictions for this agent |
| `McpServers` | `Dictionary?` | Optional MCP servers scoped to this agent |

---

## Architecture

```
  SessionConfig
  ├── McpServers (shared across session)
  │   ├── "filesystem-server" → McpLocalServerConfig
  │   └── "database-server"   → McpLocalServerConfig
  │
  └── CustomAgents
      ├── "frontend-agent" (Infer: true)
      │   └── uses shared MCP servers
      └── "data-agent" (Infer: true)
          └── McpServers (agent-scoped)
              └── "agent-db-server" → McpLocalServerConfig
```

---

## Interactive Mode

Press **Enter** to create a custom agent interactively. You provide the agent's name and system prompt, then chat with it in streaming mode.

---

## Running / Ejecución

```bash
dotnet run --project Course/demos/09.McpAgentsDemo
```

> **Note**: MCP server demos use `echo` as a no-op command since they don't require a real MCP server binary. The focus is on showing the configuration patterns. To use real MCP servers, replace `Command` and `Args` with an actual MCP server executable.
