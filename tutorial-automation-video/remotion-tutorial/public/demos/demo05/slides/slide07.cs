// Paso 6: ToolCallId en solicitudes de permisos
var toolCallIds = new List<string>();
    OnPermissionRequest = (request, invocation) =>
        if (!string.IsNullOrEmpty(request.ToolCallId))
        {
            toolCallIds.Add(request.ToolCallId);
            Console.WriteLine($"  [Permission] ToolCallId: {request.ToolCallId}");
        }
        return Task.FromResult(new PermissionRequestResult { Kind = "approved" });
await session.SendAndWaitAsync(new MessageOptions { Prompt = "Run 'echo test-toolcallid'" });
Console.WriteLine($"ToolCallIds recibidos: {toolCallIds.Count}");
// -> Cada solicitud tiene un ToolCallId unico para correlacionar