// Paso 5: Comparar multiples modelos con el mismo prompt
var models = await client.ListModelsAsync();
var modelsToTry = models.Select(m => m.Id).Take(4).ToList();
const string testPrompt = "What is the capital of Australia? One sentence.";
foreach (var modelId in modelsToTry)
    try
    {
        await using var session = await client.CreateSessionAsync(new SessionConfig { Model = modelId });
        var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = testPrompt });
        Console.WriteLine($"  {modelId,-45} -> {answer?.Data.Content?.Trim()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {modelId,-45} -> Error: {ex.Message.Split('\n')[0]}");
    }
}