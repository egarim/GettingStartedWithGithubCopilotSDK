#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 10: Mensaje de sistema - Modo Replace
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Replace,
        Content = "You are an assistant called Testy McTestface. Reply succinctly."
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your full name?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> Menciona "Testy McTestface" en lugar de "GitHub Copilot"

await client.StopAsync();
await client.DisposeAsync();
