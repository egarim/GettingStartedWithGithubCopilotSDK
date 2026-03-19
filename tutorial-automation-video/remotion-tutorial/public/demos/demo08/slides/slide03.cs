// Paso 2: Cargar y aplicar skill desde SkillDirectories
var client = new CopilotClient(new CopilotClientOptions
{
    UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();
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