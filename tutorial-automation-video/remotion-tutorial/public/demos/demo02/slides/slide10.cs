// Paso 9: Mensaje de sistema - Modo Append
await using var session = await client.CreateSessionAsync(new SessionConfig
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = "End each response with the phrase 'Have a nice day!'"
    }
});
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your name?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> Incluye "Have a nice day!" al final