// Chat con modelo por defecto
await using var session = await client.CreateSessionAsync();
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
Console.WriteLine($"  P: What model are you?");
Console.WriteLine($"  R: {answer?.Data.Content}");