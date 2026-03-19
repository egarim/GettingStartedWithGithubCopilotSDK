// Paso 5: Handler asincrono de permisos
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = async (request, invocation) =>
    {
        Console.WriteLine($"  [Permission] Kind: {request.Kind} - Verificando...");
        await Task.Delay(500); // simular verificacion asincrona (DB, API, etc.)
        Console.WriteLine("  [Permission] Aprobado tras espera");
        return new PermissionRequestResult { Kind = "approved" };
    }
});
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Run 'echo hello from async permission demo' and tell me the output"
});
Console.WriteLine($"Respuesta: {answer?.Data.Content}");