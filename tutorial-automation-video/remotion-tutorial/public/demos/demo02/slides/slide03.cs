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

async Task<AssistantMessageEvent?> WaitForIdleAsync(CopilotSession session, int timeoutSeconds = 60)
{
    AssistantMessageEvent? lastMessage = null;
    var tcs = new TaskCompletionSource<bool>();

    var sub = session.On(evt =>
    {
        if (evt is AssistantMessageEvent msg) lastMessage = msg;
        if (evt is SessionIdleEvent) tcs.TrySetResult(true);
        if (evt is SessionErrorEvent err) tcs.TrySetException(new Exception(err.Data?.Message));
    });

    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds));
    sub.Dispose();
    return lastMessage;
}