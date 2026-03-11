using System.ComponentModel;
using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

// ════════════════════════════════════════════════════════════════════════
//  11 — Bring Your Own Key (BYOK) Demo
//  Shows how to use custom models added via GitHub Copilot's BYOK feature.
//
//  Your enterprise/org admin adds an API key (e.g., OpenRouter, Anthropic,
//  Azure OpenAI, etc.) in GitHub settings, and those models appear
//  automatically in ListModelsAsync(). You use them via SessionConfig.Model
//  exactly like built-in models — no extra SDK or client needed!
//
//  See: https://docs.github.com/en/copilot/how-tos/administer-copilot/
//       manage-for-enterprise/use-your-own-api-keys
// ════════════════════════════════════════════════════════════════════════

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

Console.WriteLine("════════════════════════════════════════════════════════");
Console.WriteLine("  11 — BYOK Demo: Use Custom Models via GitHub Copilot");
Console.WriteLine("════════════════════════════════════════════════════════");
Console.WriteLine();

// ── 1. Create the client (same as every other demo) ──────────────────
Console.WriteLine("=== 1. Starting CopilotClient ===\n");

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var client = new CopilotClient(new CopilotClientOptions
{
    GithubToken = string.IsNullOrWhiteSpace(token) ? null : token,
    UseLoggedInUser = string.IsNullOrWhiteSpace(token),
    Logger = logger
});
await client.StartAsync();

var auth = await client.GetAuthStatusAsync();
Console.WriteLine($"  Auth: {auth.IsAuthenticated} ({auth.AuthType})");
Console.WriteLine($"  Source: {(string.IsNullOrWhiteSpace(token) ? "VS Code login" : "GitHub PAT")}");
Console.WriteLine();

// ── 2. List ALL available models (built-in + BYOK custom) ────────────
Console.WriteLine("=== 2. All Available Models (built-in + custom BYOK) ===\n");

var models = await client.ListModelsAsync();
Console.WriteLine($"  Total models: {models.Count}");
Console.WriteLine($"  {"ID",-45} {"Name",-30}");
Console.WriteLine($"  {"──",-45} {"────",-30}");
foreach (var m in models)
    Console.WriteLine($"  {m.Id,-45} {m.Name,-30}");
Console.WriteLine();
Console.WriteLine("  💡 Custom models from BYOK appear in this list!");
Console.WriteLine("     (Admin adds API keys at: GitHub > Enterprise > AI controls > Copilot > Custom models)");
Console.WriteLine();

// ── 3. Use the DEFAULT model ─────────────────────────────────────────
Console.WriteLine("=== 3. Chat with Default Model ===\n");
{
    await using var session = await client.CreateSessionAsync();
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
    Console.WriteLine($"  Q: What model are you?");
    Console.WriteLine($"  A: {answer?.Data.Content}");
}
Console.WriteLine();

// ── 4. Use a SPECIFIC model (pick by ID from the list above) ─────────
Console.WriteLine("=== 4. Chat with a Specific Model ===\n");

// Try claude-sonnet-4 first, fall back to gpt-4o if not available
var preferredModels = new[] { "claude-sonnet-4", "gpt-4.1", "gpt-4o" };
string? chosenModel = null;

foreach (var preferred in preferredModels)
{
    if (models.Any(m => m.Id.Equals(preferred, StringComparison.OrdinalIgnoreCase)))
    {
        chosenModel = preferred;
        break;
    }
}
chosenModel ??= models.FirstOrDefault()?.Id ?? "gpt-4o";

Console.WriteLine($"  Using model: {chosenModel}");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = chosenModel
    });
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
    Console.WriteLine($"  Q: What model are you?");
    Console.WriteLine($"  A: {answer?.Data.Content}");
}
Console.WriteLine();

// ── 5. Compare multiple models side by side ──────────────────────────
Console.WriteLine("=== 5. Compare Multiple Models ===\n");

var modelsToTry = models
    .Select(m => m.Id)
    .Take(Math.Min(models.Count, 4))
    .ToList();

const string testPrompt = "What is the capital of Australia? One sentence.";
Console.WriteLine($"  Prompt: \"{testPrompt}\"\n");

foreach (var modelId in modelsToTry)
{
    try
    {
        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Model = modelId
        });
        var answer = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = testPrompt });
        Console.WriteLine($"  {modelId,-45} → {answer?.Data.Content?.Trim()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {modelId,-45} → Error: {ex.Message.Split('\n')[0]}");
    }
}
Console.WriteLine();

// ── 6. BYOK model + Custom Tools ─────────────────────────────────────
Console.WriteLine("=== 6. Custom Model + Tools ===\n");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = chosenModel,
        Tools =
        [
            AIFunctionFactory.Create(GetWeather, "get_weather"),
            AIFunctionFactory.Create(GetTime, "get_time"),
        ]
    });

    Console.WriteLine($"  Model: {chosenModel}");
    var answer = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = "What's the weather in Tokyo and what time is it there?" });
    Console.WriteLine($"  Q: What's the weather in Tokyo and what time is it there?");
    Console.WriteLine($"  A: {answer?.Data.Content}");
}
Console.WriteLine();

// ── 7. Streaming with a custom model ─────────────────────────────────
Console.WriteLine("=== 7. Streaming with Custom Model ===\n");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = chosenModel,
        Streaming = true
    });

    Console.Write("  Streaming: ");
    var sb = new StringBuilder();
    var idleTcs = new TaskCompletionSource<bool>();

    session.On(evt =>
    {
        switch (evt)
        {
            case AssistantMessageDeltaEvent delta:
                Console.Write(delta.Data.DeltaContent);
                sb.Append(delta.Data.DeltaContent);
                break;
            case SessionIdleEvent:
                idleTcs.TrySetResult(true);
                break;
        }
    });

    await session.SendAsync(
        new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
    await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
    Console.WriteLine($"\n  Total chars: {sb.Length}");
}
Console.WriteLine();

// ── 8. Interactive chat loop ─────────────────────────────────────────
Console.WriteLine("=== 8. Interactive Chat (type 'exit' to quit) ===\n");
Console.WriteLine($"  Model: {chosenModel}");
Console.WriteLine($"  (Custom BYOK models work exactly the same!)\n");
{
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = chosenModel,
        Streaming = true
    });

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  You: ");
        Console.ResetColor();
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  AI:  ");
        Console.ResetColor();

        var sb = new StringBuilder();
        var idleTcs = new TaskCompletionSource<bool>();

        session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    Console.Write(delta.Data.DeltaContent);
                    sb.Append(delta.Data.DeltaContent);
                    break;
                case SessionIdleEvent:
                    idleTcs.TrySetResult(true);
                    break;
            }
        });

        await session.SendAsync(new MessageOptions { Prompt = input });
        await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
        Console.WriteLine("\n");
    }
}

await client.StopAsync();
await client.DisposeAsync();
Console.WriteLine("\nDone! 👋");

// ── Tool implementations ───────────────────────────────────────────────
[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_weather] city={city}");
    return $"Weather in {city}: 22°C, partly cloudy, humidity 55%";
}

[Description("Gets the current time for a city")]
static string GetTime([Description("City name")] string city)
{
    Console.WriteLine($"  [Tool:get_time] city={city}");
    return $"Current time in {city}: {DateTime.UtcNow.AddHours(9):HH:mm} JST";
}
