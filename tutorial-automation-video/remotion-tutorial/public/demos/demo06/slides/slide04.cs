// Entrada con opciones
var userInputRequests = new List<UserInputRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        userInputRequests.Add(request);
        Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
        Console.WriteLine($"    [AskUser] Session: {invocation.SessionId}");