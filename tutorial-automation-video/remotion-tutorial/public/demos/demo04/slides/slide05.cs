// Ambos hooks juntos
var preTools = new List<string>();
var postTools = new List<string>();

var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPreToolUse = (input, invocation) =>
        {
            preTools.Add(input.ToolName ?? "?");
            Console.WriteLine($"    [PRE]  -> {input.ToolName}");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
// ...