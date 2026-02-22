using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 08 – SKILLS DEMO: Skill Loading & Configuration
// 08 – DEMO DE SKILLS: Carga y configuración de habilidades
//
// Demonstrates / Demuestra:
//   • SKILL.md file format (YAML frontmatter + markdown instructions)
//   • SessionConfig.SkillDirectories — load skills from a directory
//   • SessionConfig.DisabledSkills — disable specific skills
//   • Skill effect on model behavior (marker in responses)
// ============================================================================

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));
var logger = loggerFactory.CreateLogger<CopilotClient>();

// Language selection
Console.WriteLine("================================================================");
Console.WriteLine("  Select language / Seleccione idioma:");
Console.WriteLine("  1. English");
Console.WriteLine("  2. Español");
Console.WriteLine("================================================================");
Console.Write("  Choice (1 or 2): ");
var langChoice = Console.ReadLine()?.Trim();
bool isSpanish = langChoice == "2";
Console.WriteLine();

Console.WriteLine("================================================================");
if (isSpanish)
{
    Console.WriteLine("  08 - DEMO: Carga y configuracion de habilidades");
}
else
{
    Console.WriteLine("  08 - SKILLS DEMO: Skill Loading & Configuration");
}
Console.WriteLine("================================================================");
Console.WriteLine();

// -- Create skill directory and SKILL.md / Crear directorio y SKILL.md -
var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
var skillSubdir = Path.Combine(skillsBaseDir, "demo-skill");

// Clean and recreate / Limpiar y recrear
if (Directory.Exists(skillsBaseDir)) Directory.Delete(skillsBaseDir, true);
Directory.CreateDirectory(skillSubdir);

const string SkillMarker = "PINEAPPLE_COCONUT_42";

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

if (isSpanish)
{
    Console.WriteLine("=== Anatomia del archivo SKILL.md ===");
    Console.WriteLine();
    Console.WriteLine("  Un skill se define con un archivo SKILL.md en un subdirectorio:");
}
else
{
    Console.WriteLine("=== SKILL.md Anatomy ===");
    Console.WriteLine();
    Console.WriteLine("  A skill is defined by a SKILL.md file in a subdirectory:");
}
Console.WriteLine();
Console.WriteLine($"  📁 {skillsBaseDir}/");
Console.WriteLine($"    📁 demo-skill/");
Console.WriteLine($"      📄 SKILL.md");
Console.WriteLine();
Console.WriteLine(isSpanish ? "  Contenido:" : "  SKILL.md content:");
Console.WriteLine("  +------------------------------------------------------+");
foreach (var line in skillContent.Split('\n'))
    Console.WriteLine($"  | {line,-52} |");
Console.WriteLine("  +------------------------------------------------------+");
Console.WriteLine();

var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await client.StartAsync();
Console.WriteLine(isSpanish ? "  Cliente iniciado.\n" : "  Client started.\n");

// -- 1. Load and Apply Skill / Cargar y aplicar skill ------------------
if (isSpanish)
{
    Console.WriteLine("=== 1. Cargar y aplicar skill desde SkillDirectories ===");
}
else
{
    Console.WriteLine("=== 1. Load & Apply Skill from SkillDirectories ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        SkillDirectories = [skillsBaseDir]
    });

    Console.WriteLine($"  Session: {session.SessionId}");
    Console.WriteLine($"  SkillDirectories: [\"{skillsBaseDir}\"]");
    Console.WriteLine($"  Expected marker in response: \"{SkillMarker}\"");
    Console.WriteLine();

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Say hello briefly using the demo skill."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine();

    var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
    Console.WriteLine($"  Contains marker \"{SkillMarker}\": {containsMarker}");
    if (containsMarker)
        Console.WriteLine(isSpanish ? "  OK Skill cargado y aplicado exitosamente!" : "  OK Skill was loaded and applied successfully!");
    else
        Console.WriteLine(isSpanish ? "  AVISO Marcador no encontrado - skill puede no haberse aplicado." : "  WARNING Marker not found - skill may not have been applied.");

    await session.DisposeAsync();
}
Console.WriteLine();

// -- 2. Disable Skill / Desactivar skill -------------------------------
if (isSpanish)
{
    Console.WriteLine("=== 2. Desactivar skill via DisabledSkills ===");
}
else
{
    Console.WriteLine("=== 2. Disable Skill via DisabledSkills ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig
    {
        SkillDirectories = [skillsBaseDir],
        DisabledSkills = ["demo-skill"]
    });

    Console.WriteLine($"  SkillDirectories: [\"{skillsBaseDir}\"]");
    Console.WriteLine($"  DisabledSkills: [\"demo-skill\"]");
    Console.WriteLine();

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Say hello briefly using the demo skill."
    });
    Console.WriteLine($"  Response: {answer?.Data.Content}");
    Console.WriteLine();

    var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
    Console.WriteLine($"  Contains marker \"{SkillMarker}\": {containsMarker}");
    if (!containsMarker)
        Console.WriteLine(isSpanish ? "  OK Skill desactivado correctamente - marcador ausente!" : "  OK Skill was correctly disabled - marker absent!");
    else
        Console.WriteLine(isSpanish ? "  AVISO Marcador encontrado a pesar de estar desactivado." : "  WARNING Marker found despite being disabled.");

    await session.DisposeAsync();
}
Console.WriteLine();

// -- 3. No Skill (baseline) / Sin skill (linea base) -------------------
if (isSpanish)
{
    Console.WriteLine("=== 3. Sin skill (comparacion de linea base) ===");
}
else
{
    Console.WriteLine("=== 3. No Skill (baseline comparison) ===");
}
{
    var session = await client.CreateSessionAsync(new SessionConfig());

    var answer = await session.SendAndWaitAsync(new MessageOptions
    {
        Prompt = "Say hello briefly."
    });
    Console.WriteLine($"  Response (no skill): {answer?.Data.Content}");
    Console.WriteLine($"  Contains marker: {answer?.Data.Content?.Contains(SkillMarker) ?? false}");
    Console.WriteLine(isSpanish ? "  (Esperado: Sin marcador - sin skill cargado)" : "  (Expected: No marker - no skill loaded)");

    await session.DisposeAsync();
}
Console.WriteLine();

// -- Cleanup / Limpieza ------------------------------------------------
await client.StopAsync();
await client.DisposeAsync();
try { Directory.Delete(skillsBaseDir, true); } catch { /* ignore */ }

// -- Interactive mode / Modo interactivo ------------------------------
Console.WriteLine("================================================================");
Console.WriteLine(isSpanish
    ? "  Presiona Enter para modo interactivo (¡crea tu propio skill y pruebalo!)."
    : "  Press Enter for interactive mode (create your own skill and test it!).");
Console.WriteLine("================================================================");
Console.ReadLine();

// Recreate skill dir for interactive use
Directory.CreateDirectory(skillSubdir);

Console.WriteLine(isSpanish
    ? "  Escribe tu instruccion de skill personalizada (¿que debe hacer siempre el modelo?):"
    : "  Enter your custom skill instruction (what should the model always do?):");
Console.Write(isSpanish ? "  Instruccion: " : "  Instruction: ");
var customInstruction = Console.ReadLine() ?? (isSpanish ? "Siempre termina respuestas con '🎉'" : "Always end responses with '🎉'");

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
Console.WriteLine(isSpanish
    ? $"  Skill creado en: {customSkillDir}/SKILL.md\n"
    : $"  Skill created at: {customSkillDir}/SKILL.md\n");

var chatClient = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = logger
});
await chatClient.StartAsync();

await using var chatSession = await chatClient.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    SkillDirectories = [skillsBaseDir]
});

Console.WriteLine("  Chat with your custom skill active. Type messages (empty to quit):\n");
while (true)
{
    Console.Write("  You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    chatSession.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
        if (evt is SessionErrorEvent err) { Console.WriteLine($"\n  Error: {err.Data?.Message}"); done.TrySetResult(false); }
    });

    Console.Write("  AI: ");
    await chatSession.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine();
}

await chatClient.StopAsync();
await chatClient.DisposeAsync();
try { Directory.Delete(skillsBaseDir, true); } catch { /* ignore */ }
Console.WriteLine("\n  Done! / ¡Listo!");
