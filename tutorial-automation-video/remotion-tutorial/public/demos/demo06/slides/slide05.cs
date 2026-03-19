// Paso 4: Entrada libre del usuario (WasFreeform = true)
const string freeformAnswer = "My favorite color is emerald green, a beautiful shade!";
        Console.WriteLine($"  [AskUser] Respuesta libre: \"{freeformAnswer}\"");
        return Task.FromResult(new UserInputResponse
            Answer = freeformAnswer,
            WasFreeform = true // indica que el usuario escribio texto libre
        });
var answer = await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Ask me 'What is your favorite color?' using ask_user, then include my answer."
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> Deberia mencionar "emerald green"