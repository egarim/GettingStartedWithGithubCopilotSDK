using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

// ============================================================================
// 04 – HOOKS DEMO: Pre/Post Tool-Use Hooks
// 04 – DEMO DE HOOKS: Hooks Pre/Post uso de herramientas
//
// Demonstrates / Demuestra:
//   • OnPreToolUse hook — intercept before tool execution
//   • OnPostToolUse hook — inspect results after tool execution
//   • Both hooks on one session
//   • Deny tool execution via PreToolUse
// ============================================================================

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Language selection
Console.WriteLine("================================================================");
Console.WriteLine("  Select language / Seleccione idioma:");
Console.WriteLine("  1. English");
Console.WriteLine("  2. Español");
Console.WriteLine("================================================================");
Console.Write("  Choice (1 or 2): ");
var langChoice = Console.ReadLine()?.Trim();
bool isSpanish = langChoice == "2";
Console.WriteLine();

Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  04 - DEMO: Hooks Pre/Post uso de herramientas");
}
else
{
    Console.WriteLine("  04 - HOOKS DEMO: Pre/Post Tool-Use Hooks");
}
Console.WriteLine("================================================================");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine(isSpanish ? "  Cliente iniciado.\n" : "  Client started.\n");

// We'll use a simple custom tool for the hooks to intercept
// Usaremos una herramienta personalizada simple para que los hooks intercepten
var lookupTool = AIFunctionFactory.Create(LookupPrice, "lookup_price");

// -- 1. PreToolUse Hook (Allow) / Hook Pre-uso (Permitir) -------------
if (isSpanish)
{
    Console.WriteLine("=== 1. Hook Pre-uso - Permitir ===");
}
else
{
    Console.WriteLine("=== 1. PreToolUse Hook - Allow ===");
}
{
    var preToolUseInputs = new List<string>();
    CopilotSession? session = null;
    session = await client.CreateSessionAsync(new SessionConfig
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
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? $"  Hooks disparados: {preToolUseInputs.Count}"
        : $"  PreToolUse hooks fired: {preToolUseInputs.Count}");
    Console.WriteLine($"  Tool names intercepted: [{string.Join(", ", preToolUseInputs)}]");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. PostToolUse Hook / Hook Post-uso -------------------------------
if (isSpanish)
{
    Console.WriteLine("=== 2. Hook Post-uso ===");
}
else
{
    Console.WriteLine("=== 2. PostToolUse Hook ===");
}
{
    var postToolUseInputs = new List<(string tool, string? result)>();
    CopilotSession? session = null;
    session = await client.CreateSessionAsync(new SessionConfig
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
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  PostToolUse hooks fired: {postToolUseInputs.Count}");
    foreach (var (tool, result) in postToolUseInputs)
        Console.WriteLine($"    Tool: {tool} → Result: {result?.Substring(0, Math.Min(60, result.Length))}...");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 3. Both Hooks Together / Ambos hooks juntos -----------------------
if (isSpanish)
{
    Console.WriteLine("=== 3. Ambos hooks Pre y Post juntos ===");
}
else
{
    Console.WriteLine("=== 3. Both Pre & Post Hooks Together ===");
}
{
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
                Console.WriteLine($"    [PRE]  → {input.ToolName}");
                return Task.FromResult<PreToolUseHookOutput?>(
                    new PreToolUseHookOutput { PermissionDecision = "allow" });
            },
            OnPostToolUse = (input, invocation) =>
            {
                postTools.Add(input.ToolName ?? "?");
                Console.WriteLine($"    [POST] ← {input.ToolName}");
                return Task.FromResult<PostToolUseHookOutput?>(null);
            }
        }
    });

    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "Look up the price for 'Super Deluxe Widget'." });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  Pre hooks: [{string.Join(", ", preTools)}]");
    Console.WriteLine($"  Post hooks: [{string.Join(", ", postTools)}]");

    var overlap = preTools.Intersect(postTools).ToList();
    Console.WriteLine(isSpanish
        ? $"  Misma herramienta en ambos: {overlap.Count > 0} [{string.Join(", ", overlap)}]"
        : $"  Same tool in both: {overlap.Count > 0} [{string.Join(", ", overlap)}]");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 4. Deny Tool Execution / Denegar ejecucion ------------------------
if (isSpanish)
{
    Console.WriteLine("=== 4. Denegar ejecucion via PreToolUse ===");
}
else
{
    Console.WriteLine("=== 4. Deny Tool Execution via PreToolUse ===");
}
{
    var deniedTools = new List<string>();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        Tools = [lookupTool],
        Hooks = new SessionHooks
        {
            OnPreToolUse = (input, invocation) =>
            {
                deniedTools.Add(input.ToolName ?? "?");
                Console.WriteLine(isSpanish
                    ? $"    [PreToolUse] DENEGANDO herramienta: {input.ToolName}"
                    : $"    [PreToolUse] DENYING tool: {input.ToolName}");
                return Task.FromResult<PreToolUseHookOutput?>(
                    new PreToolUseHookOutput { PermissionDecision = "deny" });
            }
        }
    });

    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "Look up the price for 'Widget Pro'. If you can't, explain why." });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine(isSpanish
        ? $"  Herramientas denegadas: {deniedTools.Count}"
        : $"  Tools denied: {deniedTools.Count}");
    Console.WriteLine(isSpanish
        ? "  (La herramienta fue bloqueada - el modelo deberia explicar que no pudo acceder)"
        : "  (The tool was blocked - the model should explain it couldn't access the tool)");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para modo interactivo (elige permitir/denegar por llamada)."
    : "  Press Enter for interactive mode (choose allow/deny per tool call).");
Console.WriteLine("================================================================");
Console.ReadLine();

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();

await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            Console.WriteLine($"\n    [Hook] Tool '{input.ToolName}' wants to execute.");
            Console.Write("    Allow? (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            var decision = response == "n" ? "deny" : "allow";
            Console.WriteLine($"    Decision: {decision.ToUpperInvariant()}");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = decision });
        },
        OnPostToolUse = (input, invocation) =>
        {
            var resStr = input.ToolResult?.ToString();
            Console.WriteLine($"    [Hook] Tool '{input.ToolName}' completed. Result: {resStr?.Substring(0, Math.Min(50, resStr.Length))}...");
            return Task.FromResult<PostToolUseHookOutput?>(null);
        }
    }
});

Console.WriteLine("  Type messages (empty to quit). You'll be prompted to allow/deny tool calls.\n");
while (true)
{
    Console.Write("  You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    chatSession.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
        if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
    });

    Console.Write("  AI: ");
    await chatSession.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine();
}

await chatClient.StopAsync();
await chatClient.DisposeAsync();
Console.WriteLine(isSpanish ? "\n  ¡Listo!" : "\n  Done!");

// -- Tool implementation / Implementacion de herramienta ---------------

[Description("Looks up the price of a product by name")]
// Busca el precio de un producto por nombre
static string LookupPrice([Description("Product name to look up")] string productName)
{
    Console.WriteLine($"    [Tool:lookup_price] productName={productName}");
    // Simulated catalog / Catálogo simulado
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
