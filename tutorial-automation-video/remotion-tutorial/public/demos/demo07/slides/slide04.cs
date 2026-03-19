// Paso 3: Compactacion desactivada - Sin eventos
var compactionEvents = new List<SessionEvent>();
    InfiniteSessions = new InfiniteSessionConfig { Enabled = false }
    if (evt is SessionCompactionStartEvent or SessionCompactionCompleteEvent)
        compactionEvents.Add(evt);
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  Eventos compactacion: {compactionEvents.Count}"); // Esperado: 0