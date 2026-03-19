// Paso 3: Chat con el modelo por defecto
await using var session = await client.CreateSessionAsync();  // modelo por defecto
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
Console.WriteLine($"  R: {answer?.Data.Content}");