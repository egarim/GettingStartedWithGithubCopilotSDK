#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Denegar permiso - Bloquear modificaciones
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
        Console.WriteLine($"  [Permission] Kind: {request.Kind} -> DENEGADO");
        return Task.FromResult(new PermissionRequestResult
        {
            Kind = "denied-interactively-by-user"
        });
    }
});

var protectedFile = Path.Combine(workDir, "protected.txt");
await File.WriteAllTextAsync(protectedFile, "protected content");

await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = $"Edit the file at {protectedFile} and replace 'protected' with 'hacked'"
});

var content = await File.ReadAllTextAsync(protectedFile);
Console.WriteLine($"Contenido intacto: {content == "protected content"}"); // -> True

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(workDir, true); } catch { }
