#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Hook PostToolUse - Inspeccionar resultado
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
        OnPostToolUse = (input, invocation) =>
        {
            Console.WriteLine($"  [PostToolUse] Tool: {input.ToolName}");
            Console.WriteLine($"  [PostToolUse] Result: {input.ToolResult}");
            return Task.FromResult<PostToolUseHookOutput?>(null);
        }
    }
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What is the price of 'Gadget X'?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> $49.95

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

[Description("Looks up the price of a product by name")]
static string LookupPrice([Description("Product name")] string productName)
{
    var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    { ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m };
    return catalog.TryGetValue(productName, out var price)
        ? $"Product: {productName}, Price: ${price}" : $"Not found.";
}
