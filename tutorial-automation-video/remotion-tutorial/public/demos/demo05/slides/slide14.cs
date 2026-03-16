// Error en handler
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine("    [Permission] A punto de LANZAR excepcion!");
        throw new InvalidOperationException("Simulated handler crash");
    }
});