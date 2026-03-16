using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new ClientDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class ClientDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle = "01 - DEMO: Ciclo de vida y conexion del cliente";
    const string Step1Text = "Creando CopilotClient (UseLoggedInUser = true)";
    const string Step2Text = "Iniciando cliente (StartAsync)";
    const string Step3Text = "Ping";
    const string Step4Text = "Estado (GetStatusAsync)";
    const string Step5Text = "Estado de autenticacion (GetAuthStatusAsync)";
    const string Step6Text = "Listar modelos disponibles (ListModelsAsync)";
    const string Step7Text = "Parada ordenada (StopAsync)";
    const string Step8Text = "Demostracion de ForceStop";
    const string SkippedNoAuth = "Omitido (no autenticado)";
    const string ForceStopIntro = "Iniciando un nuevo cliente para demostrar ForceStop...";
    const string InteractiveHint = "Presiona Enter para modo interactivo, o Ctrl+C para salir.";
    const string InteractiveHelp = "Comandos: ping, status, auth, models, quit";
    const string UnknownCmd = "Comando desconocido. Prueba: ping, status, auth, models, quit";

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

        var client = Step1_CreateClient();
        await Step2_StartClient(client);
        await Step3_Ping(client);
        await Step4_GetStatus(client);
        await client.DisposeAsync();
    }

    // ── Paso 1: Crear el cliente ────────────────────────────────────────
    CopilotClient Step1_CreateClient()
    {
        PrintStep(1, Step1Text);
        var client = CreateClient();
        PrintProp("Estado inicial:", client.State);
        Console.WriteLine();
        return client;
    }
    // ── Paso 2: Iniciar el cliente ──────────────────────────────────────
    async Task Step2_StartClient(CopilotClient client)
    {
        PrintStep(2, Step2Text);
        await client.StartAsync();
        PrintProp("Estado tras iniciar:", client.State);
        Console.WriteLine();
    }
    // ── Paso 3: Ping ────────────────────────────────────────────────────
    async Task Step3_Ping(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        var pong = await client.PingAsync("hello from demo!");
        PrintProp("Enviado:", "\"hello from demo!\"");
        PrintProp("Respuesta:", $"\"{pong.Message}\"");
        PrintProp("Timestamp:", pong.Timestamp);
        Console.WriteLine();
    }
    // ── Paso 4: Status del servidor ─────────────────────────────────────
    async Task Step4_GetStatus(CopilotClient client)
    {
        PrintStep(4, Step4Text);
        var s = await client.GetStatusAsync();
        PrintProp("Version:", s.Version);
        PrintProp("Version Protocolo:", s.ProtocolVersion);
        Console.WriteLine();
    }
}
