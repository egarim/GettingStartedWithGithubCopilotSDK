// Mensaje de sistema - Modo Replace
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Replace,
        Content = "You are an assistant called Testy McTestface. Reply succinctly."
    }
});

Console.WriteLine("  Prompt: What is your full name?");
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your full name?" });
PrintProp("Respuesta:", answer?.Data.Content);
Console.WriteLine("  (Deberia mencionar 'Testy' en lugar de 'GitHub Copilot')");