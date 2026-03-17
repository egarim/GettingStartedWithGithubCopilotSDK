// Verificar opciones
var userInputRequests = new List<UserInputRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        userInputRequests.Add(request);
        Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
        if (request.Choices is { Count: > 0 })
        {
            for (int i = 0; i < request.Choices.Count; i++)
                Console.WriteLine($"    [AskUser]   [{i + 1}] {request.Choices[i]}");
        }