// Denegar permiso
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        Console.WriteLine($"    [Permission] Kind: {request.Kind} -> DENEGADO");
        return Task.FromResult(new PermissionRequestResult
        {
            Kind = "denied-interactively-by-user"
        });
    }
});