// Comparar modelos
var modelsToTry = models.Select(m => m.Id).Take(Math.Min(models.Count, 4)).ToList();

const string testPrompt = "What is the capital of Australia? One sentence.";
Console.WriteLine($"  Prompt: \"{testPrompt}\"\n");

foreach (var modelId in modelsToTry)
{
    try
    {
        await using var session = await client.CreateSessionAsync(new SessionConfig { Model = modelId });
        var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = testPrompt });
        Console.WriteLine($"  {modelId,-45} -> {answer?.Data.Content?.Trim()}");
    }
    catch (Exception ex)
// ...