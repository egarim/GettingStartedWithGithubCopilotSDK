// Desactivar skill
var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = [skillsBaseDir],
    DisabledSkills = ["demo-skill"]
});