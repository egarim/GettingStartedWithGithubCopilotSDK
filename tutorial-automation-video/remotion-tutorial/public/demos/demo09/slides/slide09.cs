// Paso 8: Combinacion - Servidores MCP + Agentes en la misma sesion
    McpServers = new Dictionary<string, object>
        ["shared-server"] = new McpLocalServerConfig
            Type = "local", Command = "echo", Args = ["shared"], Tools = ["*"]
    },
            Name = "coordinator-agent",
            DisplayName = "Coordinator Agent",
            Description = "Coordinates tasks across MCP servers",
            Prompt = "You are a coordinator that can access shared MCP servers."
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 7+7?" });
Console.WriteLine($"  Respuesta (MCP + agente): {answer?.Data.Content}");