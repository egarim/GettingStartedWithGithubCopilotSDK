// Paso 6: Agente con sus propios servidores MCP
            Name = "data-agent",
            DisplayName = "Data Agent",
            Description = "An agent with its own MCP server",
            Prompt = "You are a data agent with database access.",
            McpServers = new Dictionary<string, object>  // MCP aislado por agente
            {
                ["agent-db-server"] = new McpLocalServerConfig
                {
                    Type = "local", Command = "echo", Args = ["agent-data"], Tools = ["*"]
                }
            }
Console.WriteLine("  Agente con MCP propio aislado");