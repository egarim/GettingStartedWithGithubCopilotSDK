#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Boilerplate - cliente y directorio de trabajo
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });

var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
Directory.CreateDirectory(workDir);

Console.WriteLine("05 - DEMO: Manejo de solicitudes de permisos");
Console.WriteLine($"Directorio de trabajo: {workDir}");

await client.DisposeAsync();
try { Directory.Delete(workDir, true); } catch { }