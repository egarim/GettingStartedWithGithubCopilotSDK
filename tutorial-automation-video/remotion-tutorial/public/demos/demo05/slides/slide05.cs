// Paso 4: Comportamiento por defecto (sin handler)
// Sin OnPermissionRequest - funciona para consultas sin escritura
var session = await client.CreateSessionAsync(new SessionConfig());
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 2+2?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> 4
// (Solo se activa para operaciones de escritura/ejecucion)