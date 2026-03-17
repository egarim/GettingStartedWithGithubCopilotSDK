// Chat con modelo especifico
PrintProp("Modelo elegido:", chosenModel);
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel
});
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What model are you? Answer in one short sentence." });
Console.WriteLine($"  P: What model are you?");
Console.WriteLine($"  R: {answer?.Data.Content}");