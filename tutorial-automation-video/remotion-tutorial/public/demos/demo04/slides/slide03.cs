// Helpers
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