// Modelo + herramientas
PrintProp("Modelo:", chosenModel);
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel,
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Tokyo and what time is it there?" });
Console.WriteLine($"  P: What's the weather in Tokyo and what time is it there?");
Console.WriteLine($"  R: {answer?.Data.Content}");