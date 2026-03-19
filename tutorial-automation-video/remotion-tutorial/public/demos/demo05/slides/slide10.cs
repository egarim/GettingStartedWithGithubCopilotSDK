// Paso 9: Chat interactivo con permisos (aprobar/denegar en vivo)
await using var session = await client.CreateSessionAsync(new SessionConfig
    Streaming = true,
        Console.WriteLine($"\n  [Permission] Kind: {request.Kind}, ToolCallId: {request.ToolCallId}");
        Console.Write("  Aprobar? (s/n): ");
        var resp = Console.ReadLine()?.Trim().ToLowerInvariant();
        var kind = (resp == "n") ? "denied-interactively-by-user" : "approved";
        return Task.FromResult(new PermissionRequestResult { Kind = kind });
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