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