// Paso 10: Modo interactivo con agente personalizado
await using var session = await client.CreateSessionAsync(new SessionConfig
    Streaming = true,
            Name = "my-agent",
            DisplayName = "My Agent",
            Description = "Custom interactive agent",
            Prompt = "You are a helpful assistant.",
            Infer = true
Console.WriteLine("Agente activo - escribe mensajes (vacio para salir):\n");
while (true)
    Console.Write("  Tu: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;
    var done = new TaskCompletionSource<bool>();
    session.On(evt =>
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
    });
    Console.Write("  IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine("\n");
}