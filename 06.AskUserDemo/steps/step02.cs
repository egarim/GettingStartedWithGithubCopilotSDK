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

        await client.StopAsync();
        await client.DisposeAsync();
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
}
