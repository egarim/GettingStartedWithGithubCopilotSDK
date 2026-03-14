using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new AskUserDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class AskUserDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "06 - DEMO: Solicitudes de entrada del usuario";
    const string Step1Text        = "Entrada con opciones (auto-responder primera opcion)";
    const string Step2Text        = "Verificar opciones en UserInputRequest";
    const string Step3Text        = "Entrada libre del usuario (WasFreeform = true)";
    const string InteractiveHint  = "Presiona Enter para modo interactivo (responde las preguntas del modelo en vivo).";
    const string InteractivePrompt = "Escribe mensajes (vacio para salir). El modelo te hara preguntas.\n";

    // ── Helpers ─────────────────────────────────────────────────────────
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

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var client = CreateClient();
        await client.StartAsync();
        Console.WriteLine("  Cliente iniciado.\n");

        await Step1_ChoiceBasedInput(client);
        await Step2_VerifyChoices(client);
        await Step3_FreeformInput(client);

        await client.StopAsync();
        await client.DisposeAsync();

        await RunInteractiveMode();
    }

    // ── Paso 1: Entrada con opciones ───────────────────────────────────
    async Task Step1_ChoiceBasedInput(CopilotClient client)
    {
        PrintStep(1, Step1Text);
        var userInputRequests = new List<UserInputRequest>();
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            OnUserInputRequest = (request, invocation) =>
            {
                userInputRequests.Add(request);
                Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
                Console.WriteLine($"    [AskUser] Session: {invocation.SessionId}");

                if (request.Choices is { Count: > 0 })
                {
                    Console.WriteLine($"    [AskUser] Opciones: [{string.Join(", ", request.Choices)}]");
                    Console.WriteLine($"    [AskUser] Auto-seleccionando primera: {request.Choices[0]}");
                    return Task.FromResult(new UserInputResponse
                    {
                        Answer = request.Choices[0],
                        WasFreeform = false
                    });
                }

                Console.WriteLine("    [AskUser] Sin opciones — respuesta libre");
                return Task.FromResult(new UserInputResponse
                {
                    Answer = "I'll go with the default option",
                    WasFreeform = true
                });
            }
        });

        Console.WriteLine("  Prompt: Ask me to choose between 'Option A' and 'Option B' using the ask_user tool.");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Ask me to choose between 'Option A' and 'Option B' using the ask_user tool. Wait for my response before continuing."
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Solicitudes recibidas:", userInputRequests.Count);
        foreach (var req in userInputRequests)
        {
            Console.WriteLine($"    Pregunta: {req.Question}");
            Console.WriteLine($"    Tiene opciones: {req.Choices is { Count: > 0 }}");
        }
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 2: Verificar opciones ─────────────────────────────────────
    async Task Step2_VerifyChoices(CopilotClient client)
    {
        PrintStep(2, Step2Text);
        var userInputRequests = new List<UserInputRequest>();
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            OnUserInputRequest = (request, invocation) =>
            {
                userInputRequests.Add(request);
                Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
                if (request.Choices is { Count: > 0 })
                {
                    for (int i = 0; i < request.Choices.Count; i++)
                        Console.WriteLine($"    [AskUser]   [{i + 1}] {request.Choices[i]}");
                }

                var answer = request.Choices?.FirstOrDefault() ?? "default";
                return Task.FromResult(new UserInputResponse { Answer = answer, WasFreeform = false });
            }
        });

        Console.WriteLine("  Prompt: Use the ask_user tool to ask me to pick between 'Red' and 'Blue'.");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Use the ask_user tool to ask me to pick between exactly two options: 'Red' and 'Blue'. These should be provided as choices. Wait for my answer."
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");

        var requestsWithChoices = userInputRequests.Where(r => r.Choices is { Count: > 0 }).ToList();
        PrintProp("Con opciones:", requestsWithChoices.Count);
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 3: Entrada libre ──────────────────────────────────────────
    async Task Step3_FreeformInput(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";
        var userInputRequests = new List<UserInputRequest>();

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            OnUserInputRequest = (request, invocation) =>
            {
                userInputRequests.Add(request);
                Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
                Console.WriteLine($"    [AskUser] Respuesta libre: \"{freeformAnswer}\"");
                return Task.FromResult(new UserInputResponse
                {
                    Answer = freeformAnswer,
                    WasFreeform = true
                });
            }
        });

        Console.WriteLine("  Prompt: Ask me 'What is your favorite color?' using ask_user, then include my answer.");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Ask me a question using ask_user: 'What is your favorite color?' Then include my answer in your response."
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
        PrintProp("Solicitudes recibidas:", userInputRequests.Count);
        Console.WriteLine("  (El modelo deberia incorporar nuestra respuesta libre sobre verde esmeralda)");
        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Modo interactivo ────────────────────────────────────────────────
    async Task RunInteractiveMode()
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {InteractiveHint}");
        Console.WriteLine("================================================================");
        Console.ReadLine();

        var client = CreateClient();
        await client.StartAsync();

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Streaming = true,
            OnUserInputRequest = (request, invocation) =>
            {
                Console.WriteLine();
                Console.WriteLine("    +- El modelo te hace una pregunta:");
                Console.WriteLine($"    |  {request.Question}");

                if (request.Choices is { Count: > 0 })
                {
                    Console.WriteLine("    |  Opciones:");
                    for (int i = 0; i < request.Choices.Count; i++)
                        Console.WriteLine($"    |    [{i + 1}] {request.Choices[i]}");
                    Console.Write("    +- Elige numero o escribe: ");
                }
                else
                {
                    Console.Write("    +- Tu respuesta: ");
                }

                var userAnswer = Console.ReadLine() ?? "";
                var wasFreeform = true;

                if (request.Choices is { Count: > 0 } && int.TryParse(userAnswer, out var idx)
                    && idx >= 1 && idx <= request.Choices.Count)
                {
                    userAnswer = request.Choices[idx - 1];
                    wasFreeform = false;
                }

                Console.WriteLine($"    -> Respondiendo: \"{userAnswer}\" (libre: {wasFreeform})");
                return Task.FromResult(new UserInputResponse { Answer = userAnswer, WasFreeform = wasFreeform });
            }
        });

        Console.WriteLine($"  {InteractivePrompt}");
        Console.WriteLine("  Prueba: \"Preguntame que lenguaje de programacion prefiero usando ask_user\"\n");

        while (true)
        {
            Console.Write("  Tu: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;

            var done = new TaskCompletionSource<bool>();
            session.On(evt =>
            {
                if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
                if (evt is SessionIdleEvent) done.TrySetResult(true);
                if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
            });

            Console.Write("  IA: ");
            await session.SendAsync(new MessageOptions { Prompt = input });
            await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("\n");
        }

        await client.StopAsync();
        await client.DisposeAsync();
        Console.WriteLine("\n  ¡Listo!");
    }
}
