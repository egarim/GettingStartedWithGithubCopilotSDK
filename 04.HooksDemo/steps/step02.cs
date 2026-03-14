using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new HooksDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class HooksDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "04 - DEMO: Hooks Pre/Post uso de herramientas";
    const string Step1Text        = "Hook Pre-uso - Permitir";
    const string Step2Text        = "Hook Post-uso";
    const string Step3Text        = "Ambos hooks Pre y Post juntos";
    const string Step4Text        = "Denegar ejecucion via PreToolUse";
    const string InteractiveHint  = "Presiona Enter para modo interactivo (elige permitir/denegar por llamada).";
    const string InteractivePrompt = "Escribe mensajes (vacio para salir). Se te pedira permitir/denegar cada llamada.\n";

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

        var lookupTool = AIFunctionFactory.Create(LookupPrice, "lookup_price");

        await Step1_PreToolUseAllow(client, lookupTool);

        await client.StopAsync();
        await client.DisposeAsync();
    }

    // ── Paso 1: PreToolUse Hook (Permitir) ──────────────────────────────
    async Task Step1_PreToolUseAllow(CopilotClient client, AIFunction lookupTool)
    {
        PrintStep(1, Step1Text);
        var preToolUseInputs = new List<string>();
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [lookupTool],
            Hooks = new SessionHooks
            {
                OnPreToolUse = (input, invocation) =>
                {
                    preToolUseInputs.Add(input.ToolName ?? "(unknown)");
                    Console.WriteLine($"    [PreToolUse] Tool: {input.ToolName}, Session: {invocation.SessionId}");
                    Console.WriteLine($"    [PreToolUse] Decision: ALLOW");
                    return Task.FromResult<PreToolUseHookOutput?>(
                        new PreToolUseHookOutput { PermissionDecision = "allow" });
                }
            }
        });

        Console.WriteLine("  Prompt: What is the price of the product 'Widget Pro'?");
        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "What is the price of the product 'Widget Pro'?" });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Hooks disparados:", preToolUseInputs.Count);
        Console.WriteLine($"  Herramientas interceptadas: [{string.Join(", ", preToolUseInputs)}]");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Implementacion de herramienta ───────────────────────────────────
    [Description("Looks up the price of a product by name")]
    static string LookupPrice([Description("Product name to look up")] string productName)
    {
        Console.WriteLine($"    [Tool:lookup_price] productName={productName}");
        var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Widget Pro"] = 29.99m,
            ["Gadget X"] = 49.95m,
            ["Super Deluxe Widget"] = 199.00m,
            ["Basic Widget"] = 9.99m,
        };
        return catalog.TryGetValue(productName, out var price)
            ? $"Product: {productName}, Price: ${price}"
            : $"Product '{productName}' not found in catalog.";
    }
}
