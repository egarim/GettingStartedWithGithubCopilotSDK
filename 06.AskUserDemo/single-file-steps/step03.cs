#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Verificar opciones en UserInputRequest
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var userInputRequests = new List<UserInputRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        userInputRequests.Add(request);
        Console.WriteLine($"  [AskUser] Pregunta: {request.Question}");
        if (request.Choices is { Count: > 0 })
        {
            for (int i = 0; i < request.Choices.Count; i++)
                Console.WriteLine($"  [AskUser]   [{i + 1}] {request.Choices[i]}");
        }
        var answer = request.Choices?.FirstOrDefault() ?? "default";
        return Task.FromResult(new UserInputResponse { Answer = answer, WasFreeform = false });
    }
});

await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Use ask_user to ask me to pick between 'Red' and 'Blue'. Wait for my answer."
});

var withChoices = userInputRequests.Where(r => r.Choices is { Count: > 0 }).ToList();
Console.WriteLine($"Solicitudes con opciones: {withChoices.Count}"); // -> 1

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
