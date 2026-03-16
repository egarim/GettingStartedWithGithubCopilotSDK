// ...
PrintProp("SkillDirectories:", $"[\"{skillsBaseDir}\"]");
PrintProp("DisabledSkills:", "[\"demo-skill\"]");

Console.WriteLine("  Prompt: Say hello briefly using the demo skill.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Say hello briefly using the demo skill."
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content}\n");