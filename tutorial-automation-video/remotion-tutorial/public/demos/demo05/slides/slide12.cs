// ToolCallId
var toolCallIds = new List<string>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    OnPermissionRequest = (request, invocation) =>
    {
        if (!string.IsNullOrEmpty(request.ToolCallId))
        {
            toolCallIds.Add(request.ToolCallId);
            Console.WriteLine($"    [Permission] ToolCallId: {request.ToolCallId}");
        }
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
    }
});