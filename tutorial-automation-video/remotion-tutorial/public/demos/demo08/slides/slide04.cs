// Cargar y aplicar skill
var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = [skillsBaseDir]
});