// ...
Console.WriteLine("  Prompt: Run 'echo resumed-with-permissions'");
await session2.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo resumed-with-permissions'"
});
Console.WriteLine($"  Handler disparado al reanudar: {permissionRequestReceived}");
await session2.DisposeAsync();