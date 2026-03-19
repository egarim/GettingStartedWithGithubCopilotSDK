// Paso 5: Chat interactivo - responder preguntas del modelo en vivo
await using var session = await client.CreateSessionAsync(new SessionConfig
    Streaming = true,
        Console.WriteLine($"\n  Pregunta del modelo: {request.Question}");
        if (request.Choices is { Count: > 0 })
            for (int i = 0; i < request.Choices.Count; i++)
                Console.WriteLine($"    [{i + 1}] {request.Choices[i]}");
            Console.Write("  Elige numero o escribe: ");
        }
        else
            Console.Write("  Tu respuesta: ");
        }
        var userAnswer = Console.ReadLine() ?? "";
        var wasFreeform = true;
        if (request.Choices is { Count: > 0 } && int.TryParse(userAnswer, out var idx)
            && idx >= 1 && idx <= request.Choices.Count)
            userAnswer = request.Choices[idx - 1];
            wasFreeform = false;
        }
        return Task.FromResult(new UserInputResponse { Answer = userAnswer, WasFreeform = wasFreeform });
Console.WriteLine("Escribe mensajes (vacio para salir):");
Console.WriteLine("Prueba: \"Preguntame que lenguaje prefiero usando ask_user\"\n");
while (true)
    Console.Write("Tu: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;
    var done = new TaskCompletionSource<bool>();
    session.On(evt =>
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
    });
    Console.Write("IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine("\n");
}