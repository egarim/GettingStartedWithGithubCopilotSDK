// Paso 3: Verificar opciones en UserInputRequest
var userInputRequests = new List<UserInputRequest>();
        userInputRequests.Add(request);
            for (int i = 0; i < request.Choices.Count; i++)
                Console.WriteLine($"  [AskUser]   [{i + 1}] {request.Choices[i]}");
        var answer = request.Choices?.FirstOrDefault() ?? "default";
        return Task.FromResult(new UserInputResponse { Answer = answer, WasFreeform = false });
await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Use ask_user to ask me to pick between 'Red' and 'Blue'. Wait for my answer."
var withChoices = userInputRequests.Where(r => r.Choices is { Count: > 0 }).ToList();
Console.WriteLine($"Solicitudes con opciones: {withChoices.Count}"); // -> 1