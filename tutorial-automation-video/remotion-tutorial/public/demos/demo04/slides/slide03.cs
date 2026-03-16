// PreToolUse Hook (Permitir)
var preToolUseInputs = new List<string>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            preToolUseInputs.Add(input.ToolName ?? "(unknown)");
            Console.WriteLine($"    [PreToolUse] Tool: {input.ToolName}, Session: {invocation.SessionId}");
            Console.WriteLine($"    [PreToolUse] Decision: ALLOW");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
        }
// ...