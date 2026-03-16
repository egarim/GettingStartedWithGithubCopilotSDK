#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 9: Mensaje de sistema - Modo Append
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
        Mode = SystemMessageMode.Append,
        Content = "End each response with the phrase 'Have a nice day!'"
    }
});

var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your name?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> Incluye "Have a nice day!" al final

await client.StopAsync();
await client.DisposeAsync();
