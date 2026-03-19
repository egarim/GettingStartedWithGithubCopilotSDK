// Paso 2: Configuracion de un servidor MCP
var session = await client.CreateSessionAsync(new SessionConfig
    McpServers = new Dictionary<string, object>
    {
        ["test-server"] = new McpLocalServerConfig
        {
            Type = "local",
            Command = "echo",
            Args = ["hello-mcp"],
            Tools = ["*"]  // todas las herramientas del servidor
        }
    }
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}"); // Sesion funciona con MCP
await session.DisposeAsync();