#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var logger = loggerFactory.CreateLogger<CopilotClient>();

CopilotClient CreateClient() => new(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});

void PrintStep(int n, string text)
    => Console.WriteLine($"=== {n}. {text} ===");

void PrintProp(string label, object? value)
    => Console.WriteLine($"  {label,-22} {value}");

Console.WriteLine("================================================================");
Console.WriteLine("  01 - DEMO: Ciclo de vida y conexion del cliente");
Console.WriteLine("================================================================\n");

// Paso 1: Crear el cliente
PrintStep(1, "Creando CopilotClient (UseLoggedInUser = true)");
var client = CreateClient();
PrintProp("Estado inicial:", client.State);
Console.WriteLine();


// Paso 2: Iniciar el cliente
PrintStep(2, "Iniciando cliente (StartAsync)");
await client.StartAsync();
PrintProp("Estado tras iniciar:", client.State);
Console.WriteLine();

await client.DisposeAsync();
