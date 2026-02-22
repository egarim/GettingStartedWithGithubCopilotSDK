using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 09 – MCP & AGENTS DEMO: MCP Servers & Custom Agents
// 09 – DEMO MCP Y AGENTES: Servidores MCP y agentes personalizados
//
// Demonstrates / Demuestra:
//   • McpLocalServerConfig — configure MCP servers on session create
//   • Multiple MCP servers
//   • CustomAgentConfig — custom agents with prompt/tools/MCP
//   • Agent with specific tools
//   • Agent with its own MCP servers
//   • Multiple agents
//   • Combined MCP + Agents configuration
//   • MCP & Agents on session resume
//
// NOTE / NOTA:
//   MCP server execution requires an actual MCP server binary. This demo
//   focuses on showing the configuration patterns. The sessions work —
//   MCP tools are simply not invoked if the server command is a no-op like "echo".
//
//   La ejecución de servidores MCP requiere un binario MCP real. Esta demo
//   se enfoca en mostrar los patrones de configuración.
// ============================================================================

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Language selection
Console.WriteLine("================================================================");
Console.WriteLine("  Select language / Seleccione idioma:");
Console.WriteLine("  1. English");
Console.WriteLine("  2. Español");
Console.WriteLine("================================================================");
Console.Write("  Choice (1 or 2): ");
var langChoice = Console.ReadLine()?.Trim();
bool isSpanish = langChoice == "2";
Console.WriteLine();

Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  09 - DEMO: Servidores MCP y agentes personalizados");
}
else
{
    Console.WriteLine("  09 - MCP & AGENTS DEMO: MCP Servers & Custom Agents");
}
Console.WriteLine("================================================================");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine(isSpanish ? "  Cliente iniciado.\n" : "  Client started.\n");

// ── 1. Single MCP Server Config / Configuración MCP simple ──────────
Console.WriteLine(isSpanish 
    ? "=== 1. Configuración de un servidor MCP ==="
    : "=== 1. Single MCP Server Configuration ===");
{
    var mcpServers = new Dictionary<string, object>
    {
        ["test-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["hello-mcp"],
            Tools = ["*"]
        }
    };

    Console.WriteLine("  McpServers config:");
    Console.WriteLine("  {");
    Console.WriteLine("    \"test-server\": {");
    Console.WriteLine("      Type: \"local\",");
    Console.WriteLine("      Command: \"echo\",");
    Console.WriteLine("      Args: [\"hello-mcp\"],");
    Console.WriteLine("      Tools: [\"*\"]   ← all tools from this server");
    Console.WriteLine("    }");
    Console.WriteLine("  }");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        McpServers = mcpServers
    });
    Console.WriteLine(isSpanish 
        ? $"  Sesión creada: {session.SessionId}"
        : $"  Session created: {session.SessionId}");

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  ✅ Sesión funciona con configuración MCP"
        : "  ✅ Session works with MCP server configuration");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 2. Multiple MCP Servers / Múltiples servidores MCP ───────────────
Console.WriteLine(isSpanish
    ? "=== 2. Múltiples servidores MCP ==="
    : "=== 2. Multiple MCP Servers ===");
{
    var mcpServers = new Dictionary<string, object>
    {
        ["filesystem-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["filesystem-server"],
            Tools = ["*"]
        },
        ["database-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["database-server"],
            Tools = ["*"]
        }
    };

    Console.WriteLine(isSpanish
        ? $"  Configurados {mcpServers.Count} servidores MCP: {string.Join(", ", mcpServers.Keys)}"
        : $"  Configured {mcpServers.Count} MCP servers: {string.Join(", ", mcpServers.Keys)}");

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        McpServers = mcpServers
    });
    Console.WriteLine(isSpanish
        ? $"  Sesión: {session.SessionId} ✅"
        : $"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 3. Custom Agent Configuration / Agente personalizado ────────────
Console.WriteLine(isSpanish
    ? "=== 3. Configuración de agente personalizado ==="
    : "=== 3. Custom Agent Configuration ===");
{
    var customAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "business-analyst",
            DisplayName = "Business Analyst Agent",
            Description = "An agent specialized in business analysis and reporting",
            Prompt = "You are a business analyst. Focus on data-driven insights, KPIs, and actionable recommendations.",
            Infer = true
        }
    };

    Console.WriteLine("  CustomAgents config:");
    Console.WriteLine("  [{");
    Console.WriteLine("    Name: \"business-analyst\",");
    Console.WriteLine("    DisplayName: \"Business Analyst Agent\",");
    Console.WriteLine("    Description: \"An agent specialized in business analysis...\",");
    Console.WriteLine("    Prompt: \"You are a business analyst...\",");
    Console.WriteLine("    Infer: true   ← model decides when to use this agent");
    Console.WriteLine("  }]");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = customAgents
    });
    Console.WriteLine(isSpanish
        ? $"  Sesión: {session.SessionId}"
        : $"  Session: {session.SessionId}");

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 5+5?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  ✅ Configuración de agente aceptada"
        : "  ✅ Agent configuration accepted");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 4. Agent with Specific Tools / Agente con herramientas ──────────
Console.WriteLine(isSpanish
    ? "=== 4. Agente con herramientas específicas ==="
    : "=== 4. Agent with Specific Tools ===");
{
    var customAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "devops-agent",
            DisplayName = "DevOps Agent",
            Description = "An agent for DevOps tasks with specific tool access",
            Prompt = "You are a DevOps agent. You can use bash and edit tools.",
            Tools = ["bash", "edit"],   // Only these tools available
            Infer = true
        }
    };

    Console.WriteLine(isSpanish
        ? "  Agente: devops-agent"
        : "  Agent: devops-agent");
    Console.WriteLine(isSpanish
        ? "  Herramientas: [\"bash\", \"edit\"] <- conjunto de herramientas restringido"
        : "  Tools: [\"bash\", \"edit\"] <- restricted tool set");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = customAgents
    });
    Console.WriteLine(isSpanish
        ? $"  Sesión: {session.SessionId} ✅"
        : $"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 5. Agent with its Own MCP Servers / Agente con sus propios MCP ──
Console.WriteLine(isSpanish
    ? "=== 5. Agente con sus propios servidores MCP ==="
    : "=== 5. Agent with its Own MCP Servers ===");
{
    var customAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "data-agent",
            DisplayName = "Data Agent",
            Description = "An agent with its own MCP server for data access",
            Prompt = "You are a data agent with access to a database MCP server.",
            McpServers = new Dictionary<string, object>
            {
                ["agent-db-server"] = new McpLocalServerConfig
                {
                    Type = "local",
                    Command = "echo",
                    Args = ["agent-data"],
                    Tools = ["*"]
                }
            }
        }
    };

    Console.WriteLine(isSpanish
        ? "  Agente: data-agent"
        : "  Agent: data-agent");
    Console.WriteLine("  Agent's McpServers: { \"agent-db-server\": { ... } }");
    Console.WriteLine(isSpanish
        ? "  (Cada agente puede tener sus propias conexiones MCP aisladas)"
        : "  (Each agent can have its own isolated MCP server connections)");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = customAgents
    });
    Console.WriteLine(isSpanish
        ? $"  Sesión: {session.SessionId} ✅"
        : $"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 6. Multiple Agents / Múltiples agentes ───────────────────────────
Console.WriteLine(isSpanish
    ? "=== 6. Múltiples agentes personalizados ==="
    : "=== 6. Multiple Custom Agents ===");
{
    var customAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "frontend-agent",
            DisplayName = "Frontend Agent",
            Description = "Specializes in React, CSS, and UI development",
            Prompt = "You are a frontend development expert."
        },
        new CustomAgentConfig
        {
            Name = "backend-agent",
            DisplayName = "Backend Agent",
            Description = "Specializes in C#, .NET, and API development",
            Prompt = "You are a backend development expert.",
            Infer = false  // Must be explicitly invoked
        }
    };

    Console.WriteLine(isSpanish
        ? $"  Configurados {customAgents.Count} agentes:"
        : $"  Configured {customAgents.Count} agents:");
    foreach (var agent in customAgents)
        Console.WriteLine($"    • {agent.Name} (Infer: {agent.Infer}) - {agent.Description}");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        CustomAgents = customAgents
    });
    Console.WriteLine(isSpanish
        ? $"  Sesión: {session.SessionId} ✅"
        : $"  Session: {session.SessionId} ✅");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 7. Combined MCP + Agents / Combinación MCP + Agentes ────────────
Console.WriteLine(isSpanish
    ? "=== 7. Combinación: Servidores MCP + Agentes personalizados ==="
    : "=== 7. Combined: MCP Servers + Custom Agents ===");
{
    var mcpServers = new Dictionary<string, object>
    {
        ["shared-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["shared"],
            Tools = ["*"]
        }
    };

    var customAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = "coordinator-agent",
            DisplayName = "Coordinator Agent",
            Description = "Coordinates tasks across MCP servers and other agents",
            Prompt = "You are a coordinator that can access shared MCP servers."
        }
    };

    Console.WriteLine("  McpServers: { \"shared-server\": { ... } }");
    Console.WriteLine("  CustomAgents: [ { name: \"coordinator-agent\", ... } ]");
    Console.WriteLine(isSpanish
        ? "  (Tanto servidores MCP como agentes en la misma sesión)"
        : "  (Both MCP servers and agents configured on the same session)");
    Console.WriteLine();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        McpServers = mcpServers,
        CustomAgents = customAgents
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 7+7?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  ✅ Configuración combinada aceptada"
        : "  ✅ Combined configuration accepted");
    await session.DisposeAsync();
}
Console.WriteLine();

// ── 8. MCP on Session Resume / MCP al reanudar sesión ────────────────
Console.WriteLine(isSpanish
    ? "=== 8. MCP y Agentes al reanudar sesión ==="
    : "=== 8. MCP & Agents on Session Resume ===");
{
    // Create session without MCP/agents
    var session1 = await client.CreateSessionAsync();
    var sessionId = session1.SessionId;
    await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What is 1+1?" });
    Console.WriteLine(isSpanish
        ? $"  Sesión creada sin MCP/agentes: {sessionId}"
        : $"  Session created without MCP/agents: {sessionId}");

    // Resume with MCP servers and agents
    var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
    {
        McpServers = new Dictionary<string, object>
        {
            ["resume-server"] = new McpLocalServerConfig
            {
                Type = "local",
                Command = "echo",
                Args = ["hello-resume"],
                Tools = ["*"]
            }
        },
        CustomAgents = new List<CustomAgentConfig>
        {
            new CustomAgentConfig
            {
                Name = "resume-agent",
                DisplayName = "Resume Agent",
                Description = "Added on resume",
                Prompt = "You are a resume agent."
            }
        }
    });

    Console.WriteLine(isSpanish
        ? $"  Sesión reanudada con MCP + Agentes: {session2.SessionId}"
        : $"  Session resumed with MCP + Agents: {session2.SessionId}");
    var answer = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? "  ✅ MCP y agentes agregados exitosamente al reanudar"
        : "  ✅ MCP and agents added successfully on resume");
    await session2.DisposeAsync();
}
Console.WriteLine();

// ── Cleanup / Limpieza ───────────────────────────────────────────────
await client.StopAsync();
await client.DisposeAsync();

// ── Interactive mode / Modo interactivo ──────────────────────────────
Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  Presiona Enter para modo interactivo con un agente personalizado, o Ctrl+C para salir.");
}
else
{
    Console.WriteLine("  Press Enter for interactive mode with a custom agent, or Ctrl+C to exit.");
}
Console.WriteLine("================================================================");
Console.ReadLine();

Console.Write(isSpanish ? "  Nombre del agente: " : "  Agent name: ");
var agentName = Console.ReadLine()?.Trim() ?? "my-agent";
Console.Write(isSpanish ? "  Instrucción del agente: " : "  Agent prompt (system instruction): ");
var agentPrompt = Console.ReadLine()?.Trim() ?? "You are a helpful assistant.";

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();

await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    CustomAgents = new List<CustomAgentConfig>
    {
        new CustomAgentConfig
        {
            Name = agentName,
            DisplayName = agentName,
            Description = $"Custom agent: {agentName}",
            Prompt = agentPrompt,
            Infer = true
        }
    }
});

Console.WriteLine(isSpanish
    ? $"\n  Agente '{agentName}' activo. Escribe mensajes (vacío para salir):\n"
    : $"\n  Agent '{agentName}' active. Type messages (empty to quit):\n");
while (true)
{
    Console.Write(isSpanish ? "  Tú: " : "  You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    chatSession.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
        if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
    });

    Console.Write("  AI: ");
    await chatSession.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine();
}

await chatClient.StopAsync();
await chatClient.DisposeAsync();
Console.WriteLine(isSpanish ? "\n  ¡Listo!" : "\n  Done!");
