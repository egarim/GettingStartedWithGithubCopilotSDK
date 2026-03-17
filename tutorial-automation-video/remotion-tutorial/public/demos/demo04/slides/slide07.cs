// Denegar ejecucion
var deniedTools = new List<string>();

var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            deniedTools.Add(input.ToolName ?? "?");
            Console.WriteLine($"    [PreToolUse] DENEGANDO herramienta: {input.ToolName}");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "deny" });
        }
// ...