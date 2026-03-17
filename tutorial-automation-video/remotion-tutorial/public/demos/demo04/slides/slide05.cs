// PostToolUse Hook
var postToolUseInputs = new List<(string tool, string? result)>();
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [lookupTool],
    Hooks = new SessionHooks
    {
        OnPostToolUse = (input, invocation) =>
        {
            var resultStr = input.ToolResult?.ToString();
            postToolUseInputs.Add((input.ToolName ?? "(unknown)", resultStr));
            Console.WriteLine($"    [PostToolUse] Tool: {input.ToolName}");
            Console.WriteLine($"    [PostToolUse] Result preview: {resultStr?.Substring(0, Math.Min(80, resultStr.Length))}");
            return Task.FromResult<PostToolUseHookOutput?>(null);
        }
// ...