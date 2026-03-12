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
        await Step2_PostToolUse(client, lookupTool);
        await Step3_BothHooks(client, lookupTool);
        await Step4_DenyExecution(client, lookupTool);

        await client.StopAsync();
        await client.DisposeAsync();

        await RunInteractiveMode();
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

        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "What is the price of the product 'Widget Pro'?" });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Hooks disparados:", preToolUseInputs.Count);
        Console.WriteLine($"  Herramientas interceptadas: [{string.Join(", ", preToolUseInputs)}]");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 2: PostToolUse Hook ────────────────────────────────────────
    async Task Step2_PostToolUse(CopilotClient client, AIFunction lookupTool)
    {
        PrintStep(2, Step2Text);
        var postToolUseInputs = new List<(string tool, string? result)>();
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [lookupTool],
            Hooks = new SessionHooks
            {
                OnPostToolUse = (input, invocation) =>
                {
                    var resultStr = input.ToolResult?.ToString();
                    postToolUseInputs.Add((input.ToolName ?? "(unknown)", resultStr));
                    Console.WriteLine($"    [PostToolUse] Tool: {input.ToolName}");
                    Console.WriteLine($"    [PostToolUse] Result preview: {resultStr?.Substring(0, Math.Min(80, resultStr.Length))}");
                    return Task.FromResult<PostToolUseHookOutput?>(null);
                }
            }
        });

        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "What is the price of the product 'Gadget X'?" });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Hooks PostToolUse:", postToolUseInputs.Count);
        foreach (var (tool, result) in postToolUseInputs)
            Console.WriteLine($"    Tool: {tool} -> Resultado: {result?.Substring(0, Math.Min(60, result.Length))}...");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 3: Ambos hooks juntos ──────────────────────────────────────
    async Task Step3_BothHooks(CopilotClient client, AIFunction lookupTool)
    {
        PrintStep(3, Step3Text);
        var preTools = new List<string>();
        var postTools = new List<string>();

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [lookupTool],
            Hooks = new SessionHooks
            {
                OnPreToolUse = (input, invocation) =>
                {
                    preTools.Add(input.ToolName ?? "?");
                    Console.WriteLine($"    [PRE]  -> {input.ToolName}");
                    return Task.FromResult<PreToolUseHookOutput?>(
                        new PreToolUseHookOutput { PermissionDecision = "allow" });
                },
                OnPostToolUse = (input, invocation) =>
                {
                    postTools.Add(input.ToolName ?? "?");
                    Console.WriteLine($"    [POST] <- {input.ToolName}");
                    return Task.FromResult<PostToolUseHookOutput?>(null);
                }
            }
        });

        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "Look up the price for 'Super Deluxe Widget'." });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        Console.WriteLine($"  Pre hooks: [{string.Join(", ", preTools)}]");
        Console.WriteLine($"  Post hooks: [{string.Join(", ", postTools)}]");

        var overlap = preTools.Intersect(postTools).ToList();
        Console.WriteLine($"  Misma herramienta en ambos: {overlap.Count > 0} [{string.Join(", ", overlap)}]");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 4: Denegar ejecucion ───────────────────────────────────────
    async Task Step4_DenyExecution(CopilotClient client, AIFunction lookupTool)
    {
        PrintStep(4, Step4Text);
        var deniedTools = new List<string>();

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Tools = [lookupTool],
            Hooks = new SessionHooks
            {
                OnPreToolUse = (input, invocation) =>
                {
                    deniedTools.Add(input.ToolName ?? "?");
                    Console.WriteLine($"    [PreToolUse] DENEGANDO herramienta: {input.ToolName}");
                    return Task.FromResult<PreToolUseHookOutput?>(
                        new PreToolUseHookOutput { PermissionDecision = "deny" });
                }
            }
        });

        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = "Look up the price for 'Widget Pro'. If you can't, explain why." });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Herramientas denegadas:", deniedTools.Count);
        Console.WriteLine("  (La herramienta fue bloqueada - el modelo deberia explicar que no pudo acceder)");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Modo interactivo ────────────────────────────────────────────────
    async Task RunInteractiveMode()
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {InteractiveHint}");
        Console.WriteLine("================================================================");
        Console.ReadLine();

        var client = CreateClient();
        await client.StartAsync();
        var lookupTool = AIFunctionFactory.Create(LookupPrice, "lookup_price");

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Streaming = true,
            Tools = [lookupTool],
            Hooks = new SessionHooks
            {
                OnPreToolUse = (input, invocation) =>
                {
                    Console.WriteLine($"\n    [Hook] La herramienta '{input.ToolName}' quiere ejecutarse.");
                    Console.Write("    Permitir? (s/n): ");
                    var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                    var decision = response == "n" ? "deny" : "allow";
                    Console.WriteLine($"    Decision: {decision.ToUpperInvariant()}");
                    return Task.FromResult<PreToolUseHookOutput?>(
                        new PreToolUseHookOutput { PermissionDecision = decision });
                },
                OnPostToolUse = (input, invocation) =>
                {
                    var resStr = input.ToolResult?.ToString();
                    Console.WriteLine($"    [Hook] Herramienta '{input.ToolName}' completada. Resultado: {resStr?.Substring(0, Math.Min(50, resStr.Length))}...");
                    return Task.FromResult<PostToolUseHookOutput?>(null);
                }
            }
        });

        Console.WriteLine($"  {InteractivePrompt}");
        while (true)
        {
            Console.Write("  Tu: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;

            var done = new TaskCompletionSource<bool>();
            session.On(evt =>
            {
                if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
                if (evt is SessionIdleEvent) done.TrySetResult(true);
                if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
            });

            Console.Write("  IA: ");
            await session.SendAsync(new MessageOptions { Prompt = input });
            await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("\n");
        }

        await client.StopAsync();
        await client.DisposeAsync();
        Console.WriteLine("\n  ¡Listo!");
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
