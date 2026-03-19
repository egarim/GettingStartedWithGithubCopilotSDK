#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Boilerplate - cliente y herramienta lookup_price
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });

var lookupTool = AIFunctionFactory.Create(LookupPrice, "lookup_price");
Console.WriteLine("04 - DEMO: Hooks Pre/Post uso de herramientas");

await client.DisposeAsync();

[Description("Looks up the price of a product by name")]
static string LookupPrice([Description("Product name")] string productName)
{
    var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m,
        ["Super Deluxe Widget"] = 199.00m, ["Basic Widget"] = 9.99m,
    };
    return catalog.TryGetValue(productName, out var price)
        ? $"Product: {productName}, Price: ${price}"
        : $"Product '{productName}' not found.";
}