#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Boilerplate - cliente y definicion de herramientas
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });

Console.WriteLine("03 - DEMO: Herramientas personalizadas (AIFunction)");

// Definir herramientas como metodos estaticos con [Description]
[Description("Encrypts a string by converting it to uppercase")]
static string EncryptString([Description("String to encrypt")] string input)
    => input.ToUpperInvariant();

await client.DisposeAsync();