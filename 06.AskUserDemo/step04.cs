#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 4: Entrada libre del usuario (WasFreeform = true)
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        Console.WriteLine($"  [AskUser] Pregunta: {request.Question}");
        Console.WriteLine($"  [AskUser] Respuesta libre: \"{freeformAnswer}\"");
        return Task.FromResult(new UserInputResponse
        {
            Answer = freeformAnswer,
            WasFreeform = true // indica que el usuario escribio texto libre
        });
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Ask me 'What is your favorite color?' using ask_user, then include my answer."
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> Deberia mencionar "emerald green"

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
