#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 2: Aprobar permiso - Operaciones de escritura
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

var workDir = Path.Combine(Path.GetTempPath(), "copilot-permissions-demo");
Directory.CreateDirectory(workDir);

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine($"  [Permission] Kind: {request.Kind} -> APROBADO");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});

var testFile = Path.Combine(workDir, "test.txt");
await File.WriteAllTextAsync(testFile, "original content");

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {testFile} and replace 'original' with 'modified'"
});
Console.WriteLine($"Contenido despues: \"{await File.ReadAllTextAsync(testFile)}\"");
// -> "modified content"

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(workDir, true); } catch { }
