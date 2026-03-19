// Paso 2: Entrada con opciones (auto-responder primera opcion)
await client.StartAsync();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        Console.WriteLine($"  [AskUser] Pregunta: {request.Question}");
        if (request.Choices is { Count: > 0 })
        {
            Console.WriteLine($"  [AskUser] Opciones: [{string.Join(", ", request.Choices)}]");
            Console.WriteLine($"  [AskUser] Auto-seleccionando: {request.Choices[0]}");
            return Task.FromResult(new UserInputResponse
            {
                Answer = request.Choices[0],
                WasFreeform = false
            });
        }
        return Task.FromResult(new UserInputResponse
        {
            Answer = "I'll go with the default option",
            WasFreeform = true
        });
    }
});
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Ask me to choose between 'Option A' and 'Option B' using the ask_user tool."
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
await session.DisposeAsync();
await client.StopAsync();