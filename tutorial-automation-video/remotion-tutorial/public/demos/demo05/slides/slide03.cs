// Aprobar permiso
var permissionRequests = new List<PermissionRequest>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        permissionRequests.Add(request);
        Console.WriteLine($"    [Permission] Kind: {request.Kind}, ToolCallId: {request.ToolCallId}");
        Console.WriteLine($"    [Permission] Session: {invocation.SessionId}");
        Console.WriteLine("    [Permission] Decision: APROBADO");
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});