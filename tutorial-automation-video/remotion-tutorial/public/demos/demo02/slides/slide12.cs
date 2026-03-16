// Reanudar sesion inexistente
try
{
    await client.ResumeSessionAsync("non-existent-session-id");
}
catch (IOException ex)
{
    PrintProp("Error esperado:", ex.Message);
}