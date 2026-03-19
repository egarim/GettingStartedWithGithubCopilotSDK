// Paso 5: Denegar ejecucion via PreToolUse
            Console.WriteLine($"  [PreToolUse] DENEGANDO: {input.ToolName}");
                new PreToolUseHookOutput { PermissionDecision = "deny" }); // bloquear
var answer = await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Look up the price for 'Widget Pro'. If you can't, explain why."
Console.WriteLine($"Respuesta: {answer?.Data.Content}");
// -> El modelo explica que no pudo acceder a la herramienta
    => $"Product: {productName}, Price: $29.99";