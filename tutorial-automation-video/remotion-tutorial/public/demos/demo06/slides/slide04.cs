// ...
        if (request.Choices is { Count: > 0 })
        {
            Console.WriteLine($"    [AskUser] Opciones: [{string.Join(", ", request.Choices)}]");
            Console.WriteLine($"    [AskUser] Auto-seleccionando primera: {request.Choices[0]}");
            return Task.FromResult(new UserInputResponse
            {
                Answer = request.Choices[0],
                WasFreeform = false
            });
        }