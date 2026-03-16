#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var logger = loggerFactory.CreateLogger<CopilotClient>();
Console.WriteLine("01 - DEMO: Ciclo de vida y conexion del cliente");
