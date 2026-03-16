using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new ClientDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class ClientDemo(ILogger<CopilotClient> logger)
{
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
        PrintTitle(Strings.DemoTitle);

        var client = Step1_CreateClient();
        await Step2_StartClient(client);
        await Step3_Ping(client);
        await client.DisposeAsync();
    }

    // ── Paso 1: Crear el cliente ────────────────────────────────────────
    CopilotClient Step1_CreateClient()
    {
        PrintStep(1, Strings.Step1Text);
        var client = CreateClient();
        PrintProp("Estado inicial:", client.State);
        Console.WriteLine();
        return client;
    }

    // ── Paso 2: Iniciar el cliente ──────────────────────────────────────
    async Task Step2_StartClient(CopilotClient client)
    {
        PrintStep(2, Strings.Step2Text);
        await client.StartAsync();
        PrintProp("Estado tras iniciar:", client.State);
        Console.WriteLine();
    }

    // ── Paso 3: Ping ────────────────────────────────────────────────────
    async Task Step3_Ping(CopilotClient client)
    {
        PrintStep(3, Strings.Step3Text);
        var pong = await client.PingAsync("hello from demo!");
        PrintProp("Enviado:", "\"hello from demo!\"");
        PrintProp("Respuesta:", $"\"{pong.Message}\"");
        PrintProp("Timestamp:", pong.Timestamp);
        Console.WriteLine();
    }
}
