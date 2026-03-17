// Multiples herramientas
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "What's the weather in Madrid and what time is it there?" });
Console.WriteLine($"  Prompt:   What's the weather in Madrid and what time is it there?");
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
await session.DisposeAsync();