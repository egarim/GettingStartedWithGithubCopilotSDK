#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Denegar ejecucion via PreToolUse
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var lookupTool = AIFunctionFactory.Create(LookupPrice, "lookup_price");
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            Console.WriteLine($"  [PreToolUse] DENEGANDO: {input.ToolName}");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "deny" }); // bloquear
        }
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Look up the price for 'Widget Pro'. If you can't, explain why."
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> El modelo explica que no pudo acceder a la herramienta

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

[Description("Looks up the price of a product by name")]
static string LookupPrice([Description("Product name")] string productName)
    => $"Product: {productName}, Price: $29.99";
