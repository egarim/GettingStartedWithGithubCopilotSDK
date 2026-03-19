#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 2: Cargar y aplicar skill desde SkillDirectories
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();

var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");

var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = [skillsBaseDir]  // carga todos los SKILL.md del directorio
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly using the demo skill."
});
var containsMarker = answer?.Data.Content?.Contains("PINEAPPLE_COCONUT_42") ?? false;
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  Contiene marcador: {containsMarker}"); // Esperado: True

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
