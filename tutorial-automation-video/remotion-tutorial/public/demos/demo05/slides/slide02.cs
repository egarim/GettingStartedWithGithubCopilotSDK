// Paso 0: Estructura base
#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var logger = loggerFactory.CreateLogger<CopilotClient>();

// dotnet run step01.cs