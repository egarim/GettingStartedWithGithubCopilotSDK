#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 5: Modo interactivo con skill personalizado
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
var customSkillDir = Path.Combine(skillsBaseDir, "custom-skill");
Directory.CreateDirectory(customSkillDir);

await File.WriteAllTextAsync(Path.Combine(customSkillDir, "SKILL.md"), """
    ---
    name: custom-skill
    description: A user-defined custom skill
    ---
    Siempre termina respuestas con '!'
    """);

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    SkillDirectories = [skillsBaseDir]  // carga todos los skills del directorio
});

Console.WriteLine("Chatea con tu skill personalizado (vacio para salir):\n");
while (true)
{
    Console.Write("  Tu: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    session.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
    });

    Console.Write("  IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine("\n");
}

await client.StopAsync();
await client.DisposeAsync();
