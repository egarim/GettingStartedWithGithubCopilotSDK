#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 3: Desactivar skill via DisabledSkills
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
    SkillDirectories = [skillsBaseDir],
    DisabledSkills = ["demo-skill"]  // desactiva por nombre
});

var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly using the demo skill."
});
var containsMarker = answer?.Data.Content?.Contains("PINEAPPLE_COCONUT_42") ?? false;
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  Contiene marcador: {containsMarker}"); // Esperado: False

await session.DisposeAsync();
await client.StopAsync();
await client.DisposeAsync();
