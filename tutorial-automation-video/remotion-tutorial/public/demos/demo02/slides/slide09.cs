// Paso 8: Reanudar sesion inexistente (manejo de errores)
try
{
    await client.ResumeSessionAsync("non-existent-session-id");
}
catch (IOException ex)
{
    Console.WriteLine($"Error esperado: {ex.Message}"); // -> IOException
}