// Entrada libre
const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";
var userInputRequests = new List<UserInputRequest>();

var session = await client.CreateSessionAsync(new SessionConfig
{
    OnUserInputRequest = (request, invocation) =>
    {
        userInputRequests.Add(request);
        Console.WriteLine($"    [AskUser] Pregunta: {request.Question}");
        Console.WriteLine($"    [AskUser] Respuesta libre: \"{freeformAnswer}\"");
        return Task.FromResult(new UserInputResponse
        {
            Answer = freeformAnswer,
            WasFreeform = true
// ...