// Paso 3: Hook PostToolUse - Inspeccionar resultado
        OnPostToolUse = (input, invocation) =>
            Console.WriteLine($"  [PostToolUse] Tool: {input.ToolName}");
            Console.WriteLine($"  [PostToolUse] Result: {input.ToolResult}");
            return Task.FromResult<PostToolUseHookOutput?>(null);
    new MessageOptions { Prompt = "What is the price of 'Gadget X'?" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> $49.95