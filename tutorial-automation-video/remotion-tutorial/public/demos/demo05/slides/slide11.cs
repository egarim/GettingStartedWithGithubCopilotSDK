// Handler asincrono
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = async (request, invocation) =>
    {
        Console.WriteLine($"    [Permission] Kind: {request.Kind} - Simulando verificacion asincrona...");
        await Task.Delay(500);
        Console.WriteLine("    [Permission] Aprobado tras espera");
        return new PermissionRequestResult { Kind = "approved" };
    }
});