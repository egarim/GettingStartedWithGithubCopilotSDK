// Paso 4: Sin skill (comparacion de linea base)
var session = await client.CreateSessionAsync(new SessionConfig());  // sin SkillDirectories
    Prompt = "Say hello briefly."
Console.WriteLine($"  Respuesta (sin skill): {answer?.Data.Content}");