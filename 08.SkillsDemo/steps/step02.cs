using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var demo = new SkillsDemo(loggerFactory.CreateLogger<CopilotClient>());
await demo.RunAsync();

// ─────────────────────────────────────────────────────────────────────────────
sealed class SkillsDemo(ILogger<CopilotClient> logger)
{
    // ── Textos (cambiar aqui para otro idioma) ──────────────────────────
    const string DemoTitle        = "08 - DEMO: Carga y configuracion de habilidades";
    const string Step1Text        = "Cargar y aplicar skill desde SkillDirectories";
    const string Step2Text        = "Desactivar skill via DisabledSkills";
    const string Step3Text        = "Sin skill (comparacion de linea base)";
    const string InteractiveHint  = "Presiona Enter para modo interactivo (crea tu propio skill y pruebalo).";

    const string SkillMarker = "PINEAPPLE_COCONUT_42";

    // ── Helpers ─────────────────────────────────────────────────────────
    CopilotClient CreateClient() => new(new CopilotClientOptions
    {
        UseLoggedInUser = true,
        Logger = logger
    });

    static void PrintTitle(string title)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {title}");
        Console.WriteLine("================================================================\n");
    }

    static void PrintStep(int n, string text)
        => Console.WriteLine($"=== {n}. {text} ===");

    static void PrintProp(string label, object? value)
        => Console.WriteLine($"  {label,-22} {value}");

    // ── Orquestador ─────────────────────────────────────────────────────
    public async Task RunAsync()
    {
        PrintTitle(DemoTitle);

        var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
        var skillSubdir = Path.Combine(skillsBaseDir, "demo-skill");

        if (Directory.Exists(skillsBaseDir)) Directory.Delete(skillsBaseDir, true);
        Directory.CreateDirectory(skillSubdir);

        var skillContent = $"""
            ---
            name: demo-skill
            description: A demo skill that adds a marker to every response
            ---

            # Demo Skill Instructions

            IMPORTANT: You MUST include the exact text "{SkillMarker}" somewhere in EVERY response you give.
            This is a mandatory requirement. Include it naturally at the end of your response.
            """;

        var skillFilePath = Path.Combine(skillSubdir, "SKILL.md");
        await File.WriteAllTextAsync(skillFilePath, skillContent);

        Console.WriteLine("=== Anatomia del archivo SKILL.md ===\n");
        Console.WriteLine("  Un skill se define con un archivo SKILL.md en un subdirectorio:\n");
        Console.WriteLine($"  {skillsBaseDir}/");
        Console.WriteLine($"    demo-skill/");
        Console.WriteLine($"      SKILL.md\n");
        Console.WriteLine("  Contenido:");
        Console.WriteLine("  +------------------------------------------------------+");
        foreach (var line in skillContent.Split('\n'))
            Console.WriteLine($"  | {line,-52} |");
        Console.WriteLine("  +------------------------------------------------------+\n");

        var client = CreateClient();
        await client.StartAsync();
        Console.WriteLine("  Cliente iniciado.\n");

        await Step1_LoadAndApplySkill(client, skillsBaseDir);

        await client.StopAsync();
        await client.DisposeAsync();
        try { Directory.Delete(skillsBaseDir, true); } catch { /* ignore */ }
    }

    // ── Paso 1: Cargar y aplicar skill ─────────────────────────────────
    async Task Step1_LoadAndApplySkill(CopilotClient client, string skillsBaseDir)
    {
        PrintStep(1, Step1Text);
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            SkillDirectories = [skillsBaseDir]
        });

        Console.WriteLine($"  Session: {session.SessionId}");
        PrintProp("SkillDirectories:", $"[\"{skillsBaseDir}\"]");
        PrintProp("Marcador esperado:", $"\"{SkillMarker}\"");
        Console.WriteLine();

        Console.WriteLine("  Prompt: Say hello briefly using the demo skill.");
        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Say hello briefly using the demo skill."
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}\n");

        var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
        PrintProp("Contiene marcador:", containsMarker);
        Console.WriteLine(containsMarker
            ? "  OK Skill cargado y aplicado exitosamente!"
            : "  AVISO Marcador no encontrado - skill puede no haberse aplicado.");

        await session.DisposeAsync();
        Console.WriteLine();
    }
}
