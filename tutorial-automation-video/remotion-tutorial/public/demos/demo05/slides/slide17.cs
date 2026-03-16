// ...
var permissionRequestReceived = false;
var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        permissionRequestReceived = true;
        Console.WriteLine($"    [Permission on Resume] Kind: {request.Kind} -> APROBADO");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});