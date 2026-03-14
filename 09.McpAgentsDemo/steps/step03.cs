using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new McpAgentsDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class McpAgentsDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "09 - DEMO: Servidores MCP y agentes personalizados";
    const string Step1Text        = "Configuracion de un servidor MCP";
    const string Step2Text        = "Multiples servidores MCP";
    const string Step3Text        = "Configuracion de agente personalizado";
    const string Step4Text        = "Agente con herramientas especificas";
    const string Step5Text        = "Agente con sus propios servidores MCP";
    const string Step6Text        = "Multiples agentes personalizados";
    const string Step7Text        = "Combinacion: Servidores MCP + Agentes";
    const string Step8Text        = "MCP y Agentes al reanudar sesion";
    const string InteractiveHint  = "Presiona Enter para modo interactivo con un agente personalizado.";

    // ── Helpers ─────────────────────────────────────────────────────────
    CopilotClient CreateClient() => new(new CopilotClientOptions
    {
        UseLoggedInUser = true,
        Logger = logger
    });

    static void PrintTitle(string title)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {title}");
        Console.WriteLine("================================================================\n");
    }

    static void PrintStep(int n, string text)
        => Console.WriteLine($"=== {n}. {text} ===");

    static void PrintProp(string label, object? value)
        => Console.WriteLine($"  {label,-22} {value}");

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var client = CreateClient();
        await client.StartAsync();
        Console.WriteLine("  Cliente iniciado.\n");

        await Step1_SingleMcpServer(client);
        await Step2_MultipleMcpServers(client);

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Paso 1: Servidor MCP simple ────────────────────────────────────
    async Task Step1_SingleMcpServer(CopilotClient client)
    {
        PrintStep(1, Step1Text);
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
        Console.WriteLine("      Tools: [\"*\"]   <- todas las herramientas del servidor");
        Console.WriteLine("    }");
        Console.WriteLine("  }\n");

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            McpServers = mcpServers
        });
        PrintProp("Sesion creada:", session.SessionId);

        Console.WriteLine("  Prompt: What is 2+2?");
        var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        Console.WriteLine("  Sesion funciona con configuracion MCP");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 2: Multiples servidores MCP ───────────────────────────────
    async Task Step2_MultipleMcpServers(CopilotClient client)
    {
        PrintStep(2, Step2Text);
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

        PrintProp("Servidores MCP:", $"{mcpServers.Count}: {string.Join(", ", mcpServers.Keys)}");

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            McpServers = mcpServers
        });
        PrintProp("Sesion:", $"{session.SessionId}");
        await session.DisposeAsync();
        Console.WriteLine();
    }
}
