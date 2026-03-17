// Mensaje de sistema - Modo Append
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = "End each response with the phrase 'Have a nice day!'"
    }
});

Console.WriteLine("  Prompt: What is your name?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your name?" });
PrintProp("Respuesta:", answer?.Data.Content);
Console.WriteLine($"  Contiene 'Have a nice day!': {answer?.Data.Content?.Contains("Have a nice day!") ?? false}");