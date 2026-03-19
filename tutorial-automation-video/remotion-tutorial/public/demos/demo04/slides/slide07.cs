// Paso 6: Chat interactivo con hooks (permitir/denegar por llamada)
await using var session = await client.CreateSessionAsync(new SessionConfig
    Streaming = true,
            Console.Write($"\n  [Hook] '{input.ToolName}' quiere ejecutarse. Permitir? (s/n): ");
            var resp = Console.ReadLine()?.Trim().ToLowerInvariant();
            var decision = resp == "n" ? "deny" : "allow";
                new PreToolUseHookOutput { PermissionDecision = decision });
        },
        OnPostToolUse = (input, invocation) =>
            Console.WriteLine($"  [Hook] '{input.ToolName}' completada.");
            return Task.FromResult<PostToolUseHookOutput?>(null);
Console.WriteLine("Escribe mensajes (vacio para salir):\n");
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
    var catalog = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    { ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m, ["Super Deluxe Widget"] = 199.00m };
    return catalog.TryGetValue(productName, out var price)
        ? $"Product: {productName}, Price: ${price}" : $"Not found.";
}