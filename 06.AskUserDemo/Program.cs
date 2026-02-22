using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 06 – ASK USER DEMO: User Input Requests
// 06 – DEMO PREGUNTAR AL USUARIO: Solicitudes de entrada del usuario
//
// Demonstrates / Demuestra:
//   • OnUserInputRequest handler — respond to model's questions
//   • Choice-based prompts (UserInputRequest.Choices)
//   • Freeform user input (WasFreeform = true)
//   • SessionId validation via ToolInvocation
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
    Console.WriteLine("  06 - DEMO: Solicitudes de entrada del usuario");
}
else
{
    Console.WriteLine("  06 - ASK USER DEMO: User Input Requests");
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

// -- 1. Choice-based User Input / Entrada con opciones -----------------
if (isSpanish)
{
    Console.WriteLine("=== 1. Entrada con opciones (auto-responder primera opcion) ===");
}
else
{
    Console.WriteLine("=== 1. Choice-based User Input (auto-answer first choice) ===");
}
{
    var userInputRequests = new List<UserInputRequest>();
    CopilotSession? session = null;
    session = await client.CreateSessionAsync(new SessionConfig
    {
        OnUserInputRequest = (request, invocation) =>
        {
            userInputRequests.Add(request);
            Console.WriteLine($"    [AskUser] Question: {request.Question}");
            Console.WriteLine($"    [AskUser] Session: {invocation.SessionId}");

            if (request.Choices is { Count: > 0 })
            {
                Console.WriteLine($"    [AskUser] Choices: [{string.Join(", ", request.Choices)}]");
                Console.WriteLine($"    [AskUser] Auto-selecting first choice: {request.Choices[0]}");
                return Task.FromResult(new UserInputResponse
                {
                    Answer = request.Choices[0],
                    WasFreeform = false
                });
            }

            Console.WriteLine("    [AskUser] No choices — returning freeform answer");
            return Task.FromResult(new UserInputResponse
            {
                Answer = "I'll go with the default option",
                WasFreeform = true
            });
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Ask me to choose between 'Option A' and 'Option B' using the ask_user tool. Wait for my response before continuing."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  UserInputRequests received: {userInputRequests.Count}");
    foreach (var req in userInputRequests)
    {
        Console.WriteLine($"    Question: {req.Question}");
        Console.WriteLine($"    Has choices: {req.Choices is { Count: > 0 }}");
    }
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. Verify Choices are Received / Verificar opciones recibidas -----
if (isSpanish)
{
    Console.WriteLine("=== 2. Verificar opciones en UserInputRequest ===");
}
else
{
    Console.WriteLine("=== 2. Verify Choices in UserInputRequest ===");
}
{
    var userInputRequests = new List<UserInputRequest>();
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnUserInputRequest = (request, invocation) =>
        {
            userInputRequests.Add(request);
            Console.WriteLine($"    [AskUser] Question: {request.Question}");
            if (request.Choices is { Count: > 0 })
            {
                for (int i = 0; i < request.Choices.Count; i++)
                    Console.WriteLine($"    [AskUser]   [{i + 1}] {request.Choices[i]}");
            }

            var answer = request.Choices?.FirstOrDefault() ?? "default";
            return Task.FromResult(new UserInputResponse { Answer = answer, WasFreeform = false });
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Use the ask_user tool to ask me to pick between exactly two options: 'Red' and 'Blue'. These should be provided as choices. Wait for my answer."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content}");

    var requestsWithChoices = userInputRequests.Where(r => r.Choices is { Count: > 0 }).ToList();
    Console.WriteLine(isSpanish
        ? $"  Con opciones: {requestsWithChoices.Count}"
        : $"  Requests with choices: {requestsWithChoices.Count}");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- 3. Freeform User Input / Entrada libre ----------------------------
if (isSpanish)
{
    Console.WriteLine("=== 3. Entrada libre del usuario (WasFreeform = true) ===");
}
else
{
    Console.WriteLine("=== 3. Freeform User Input (WasFreeform = true) ===");
}
{
    const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";
    var userInputRequests = new List<UserInputRequest>();

    var session = await client.CreateSessionAsync(new SessionConfig
    {
        OnUserInputRequest = (request, invocation) =>
        {
            userInputRequests.Add(request);
            Console.WriteLine($"    [AskUser] Question: {request.Question}");
            Console.WriteLine($"    [AskUser] Freeform answer: \"{freeformAnswer}\"");
            return Task.FromResult(new UserInputResponse
            {
                Answer = freeformAnswer,
                WasFreeform = true
            });
        }
    });

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Ask me a question using ask_user: 'What is your favorite color?' Then include my answer in your response."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine($"  UserInputRequests: {userInputRequests.Count}");
    Console.WriteLine(isSpanish
        ? "  (El modelo deberia incorporar nuestra respuesta libre sobre verde esmeralda)"
        : "  (The model should incorporate our freeform answer about emerald green)");
    await session.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para modo interactivo (¡responde las preguntas del modelo en vivo!)."
    : "  Press Enter for interactive mode (answer the model's questions live!).");
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
    OnUserInputRequest = (request, invocation) =>
    {
        Console.WriteLine();
        Console.WriteLine(isSpanish
            ? "    +- El modelo te hace una pregunta:"
            : "    +- The model is asking you a question:");
        Console.WriteLine($"    |  {request.Question}");

        if (request.Choices is { Count: > 0 })
        {
            Console.WriteLine(isSpanish ? "    |  Opciones:" : "    |  Choices:");
            for (int i = 0; i < request.Choices.Count; i++)
                Console.WriteLine($"    |    [{i + 1}] {request.Choices[i]}");
            Console.Write(isSpanish
                ? "    +- Elige numero o escribe: "
                : "    +- Pick a number or type your own answer: ");
        }
        else
        {
            Console.Write(isSpanish ? "    +- Tu respuesta: " : "    +- Your answer: ");
        }

        var userAnswer = Console.ReadLine() ?? "";
        var wasFreeform = true;

        // If they typed a number matching a choice, use that choice
        if (request.Choices is { Count: > 0 } && int.TryParse(userAnswer, out var idx)
            && idx >= 1 && idx <= request.Choices.Count)
        {
            userAnswer = request.Choices[idx - 1];
            wasFreeform = false;
        }

        Console.WriteLine(isSpanish
            ? $"    -> Respondiendo: \"{userAnswer}\" (libre: {wasFreeform})"
            : $"    -> Answering: \"{userAnswer}\" (freeform: {wasFreeform})");
        return Task.FromResult(new UserInputResponse { Answer = userAnswer, WasFreeform = wasFreeform });
    }
});

Console.WriteLine("  Type prompts that ask the model to ask YOU questions.\n");
Console.WriteLine("  Try: \"Ask me what programming language I prefer using ask_user\"\n");

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
