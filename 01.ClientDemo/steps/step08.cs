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
        await Step4_GetStatus(client);
        var isAuth = await Step5_GetAuthStatus(client);
        await Step6_ListModels(client, isAuth);
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

    // ── Paso 4: Status del servidor ─────────────────────────────────────
    async Task Step4_GetStatus(CopilotClient client)
    {
        PrintStep(4, Strings.Step4Text);
        var s = await client.GetStatusAsync();
        PrintProp("Version:", s.Version);
        PrintProp("Version Protocolo:", s.ProtocolVersion);
        Console.WriteLine();
    }

    // ── Paso 5: Estado de autenticacion ─────────────────────────────────
    async Task<bool> Step5_GetAuthStatus(CopilotClient client)
    {
        PrintStep(5, Strings.Step5Text);
        var auth = await client.GetAuthStatusAsync();
        PrintProp("Autenticado:", auth.IsAuthenticated);
        PrintProp("Tipo:", auth.AuthType ?? "");
        PrintProp("Mensaje:", auth.StatusMessage ?? "");
        Console.WriteLine();
        return auth.IsAuthenticated;
    }

    // ── Paso 6: Listar modelos ──────────────────────────────────────────
    async Task Step6_ListModels(CopilotClient client, bool isAuth)
    {
        PrintStep(6, Strings.Step6Text);
        if (!isAuth) { Console.WriteLine($"  {Strings.SkippedNoAuth}"); Console.WriteLine(); return; }

        var models = await client.ListModelsAsync();
        Console.WriteLine($"  Se encontraron {models.Count} modelo(s):");
        Console.WriteLine($"  {"ID",-35} {"Name",-25} {"Capabilities"}");
        Console.WriteLine($"  {"--",-35} {"----",-25} {"------------"}");
        foreach (var m in models)
            Console.WriteLine($"  {m.Id,-35} {m.Name,-25} {m.Capabilities?.ToString() ?? ""}");
        Console.WriteLine();
    }
}
