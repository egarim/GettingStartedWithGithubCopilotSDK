// Paso 7: Error en handler - Degradacion elegante
        Console.WriteLine("  [Permission] Lanzando excepcion!");
        throw new InvalidOperationException("Simulated handler crash");
var answer = await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Run 'echo test'. If you can't, say 'failed'."
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> El SDK maneja la excepcion: permiso denegado automaticamente