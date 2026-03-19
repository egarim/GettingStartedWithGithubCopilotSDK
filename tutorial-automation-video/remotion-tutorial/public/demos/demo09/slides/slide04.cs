// Paso 3: Multiples servidores MCP en una sesion
        ["filesystem-server"] = new McpLocalServerConfig
            Type = "local", Command = "echo", Args = ["filesystem"], Tools = ["*"]
        },
        ["database-server"] = new McpLocalServerConfig
            Type = "local", Command = "echo", Args = ["database"], Tools = ["*"]
Console.WriteLine($"  Sesion con 2 servidores MCP: {session.SessionId}");