#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 2: Herramienta personalizada simple (encrypt_string)
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Use encrypt_string to encrypt: Hello World" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> HELLO WORLD

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();

[Description("Encrypts a string by converting it to uppercase")]
static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();
