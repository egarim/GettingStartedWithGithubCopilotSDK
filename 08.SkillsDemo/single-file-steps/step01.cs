#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Crear archivo SKILL.md para definir un skill
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

var skillsBaseDir = Path.Combine(Path.GetTempPath(), "copilot-skills-demo");
var skillSubdir = Path.Combine(skillsBaseDir, "demo-skill");
Directory.CreateDirectory(skillSubdir);

var skillContent = """
    ---
    name: demo-skill
    description: A demo skill that adds a marker to every response
    ---
    IMPORTANT: You MUST include "PINEAPPLE_COCONUT_42" in EVERY response.
    """;

await File.WriteAllTextAsync(Path.Combine(skillSubdir, "SKILL.md"), skillContent);
Console.WriteLine($"  Skill creado en: {skillSubdir}/SKILL.md");
// Estructura: skillsBaseDir/demo-skill/SKILL.md
