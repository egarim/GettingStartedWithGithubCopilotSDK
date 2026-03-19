// Paso 2: Hook PreToolUse - Permitir ejecucion
await client.StartAsync();
var session = await client.CreateSessionAsync(new SessionConfig
    Tools = [lookupTool],
    Hooks = new SessionHooks
        OnPreToolUse = (input, invocation) =>
        {
            Console.WriteLine($"  [PreToolUse] Tool: {input.ToolName} -> ALLOW");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
        }
    }
});
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What is the price of 'Widget Pro'?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> $29.99
await session.DisposeAsync();
    { ["Widget Pro"] = 29.99m, ["Gadget X"] = 49.95m };
        ? $"Product: {productName}, Price: ${price}" : $"Not found.";