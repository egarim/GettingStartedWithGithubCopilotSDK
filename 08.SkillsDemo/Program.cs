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

        // Crear directorio de skills y SKILL.md
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
        await Step2_DisableSkill(client, skillsBaseDir);
        await Step3_NoSkillBaseline(client);

        await client.StopAsync();
        await client.DisposeAsync();
        try { Directory.Delete(skillsBaseDir, true); } catch { /* ignore */ }

        await RunInteractiveMode(skillsBaseDir, skillSubdir);
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

    // ── Paso 2: Desactivar skill ───────────────────────────────────────
    async Task Step2_DisableSkill(CopilotClient client, string skillsBaseDir)
    {
        PrintStep(2, Step2Text);
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            SkillDirectories = [skillsBaseDir],
            DisabledSkills = ["demo-skill"]
        });

        PrintProp("SkillDirectories:", $"[\"{skillsBaseDir}\"]");
        PrintProp("DisabledSkills:", "[\"demo-skill\"]");
        Console.WriteLine();

        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Say hello briefly using the demo skill."
        });
        Console.WriteLine($"  Respuesta: {answer?.Data.Content}\n");

        var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
        PrintProp("Contiene marcador:", containsMarker);
        Console.WriteLine(!containsMarker
            ? "  OK Skill desactivado correctamente - marcador ausente!"
            : "  AVISO Marcador encontrado a pesar de estar desactivado.");

        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Paso 3: Sin skill (linea base) ─────────────────────────────────
    async Task Step3_NoSkillBaseline(CopilotClient client)
    {
        PrintStep(3, Step3Text);
        var session = await client.CreateSessionAsync(new SessionConfig());

        var answer = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = "Say hello briefly."
        });
        Console.WriteLine($"  Respuesta (sin skill): {answer?.Data.Content}");
        PrintProp("Contiene marcador:", answer?.Data.Content?.Contains(SkillMarker) ?? false);
        Console.WriteLine("  (Esperado: Sin marcador - sin skill cargado)");

        await session.DisposeAsync();
        Console.WriteLine();
    }

    // ── Modo interactivo ────────────────────────────────────────────────
    async Task RunInteractiveMode(string skillsBaseDir, string skillSubdir)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine($"  {InteractiveHint}");
        Console.WriteLine("================================================================");
        Console.ReadLine();

        Directory.CreateDirectory(skillSubdir);

        Console.WriteLine("  Escribe tu instruccion de skill personalizada (que debe hacer siempre el modelo?):");
        Console.Write("  Instruccion: ");
        var customInstruction = Console.ReadLine() ?? "Siempre termina respuestas con '!'";

        var customSkillContent = $"""
            ---
            name: custom-skill
            description: A user-defined custom skill
            ---

            # Custom Skill

            {customInstruction}
            """;

        var customSkillDir = Path.Combine(skillsBaseDir, "custom-skill");
        Directory.CreateDirectory(customSkillDir);
        await File.WriteAllTextAsync(Path.Combine(customSkillDir, "SKILL.md"), customSkillContent);
        Console.WriteLine($"  Skill creado en: {customSkillDir}/SKILL.md\n");

        var client = CreateClient();
        await client.StartAsync();

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Streaming = true,
            SkillDirectories = [skillsBaseDir]
        });

        Console.WriteLine("  Chatea con tu skill personalizado activo. Escribe mensajes (vacio para salir):\n");
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
                if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
            });

            Console.Write("  IA: ");
            await session.SendAsync(new MessageOptions { Prompt = input });
            await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("\n");
        }

        await client.StopAsync();
        await client.DisposeAsync();
        try { Directory.Delete(skillsBaseDir, true); } catch { /* ignore */ }
        Console.WriteLine("\n  ¡Listo!");
    }
}
