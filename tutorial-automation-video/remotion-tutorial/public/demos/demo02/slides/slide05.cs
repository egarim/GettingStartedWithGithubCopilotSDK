// ...
await session.DisposeAsync();
Console.WriteLine("  Sesion destruida.");

try
{
    await session.GetMessagesAsync();
}
catch (IOException ex)
{
    PrintProp("Error esperado:", ex.Message);
}