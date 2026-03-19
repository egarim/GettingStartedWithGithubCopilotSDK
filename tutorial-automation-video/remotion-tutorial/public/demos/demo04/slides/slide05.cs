// Paso 4: Ambos hooks Pre y Post juntos
        OnPreToolUse = (input, invocation) =>
            Console.WriteLine($"  [PRE]  -> {input.ToolName}");
            return Task.FromResult<PreToolUseHookOutput?>(
                new PreToolUseHookOutput { PermissionDecision = "allow" });
        },
            Console.WriteLine($"  [POST] <- {input.ToolName}: {input.ToolResult}");
    new MessageOptions { Prompt = "Look up the price for 'Super Deluxe Widget'." });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> $199.00
    { ["Widget Pro"] = 29.99m, ["Super Deluxe Widget"] = 199.00m };