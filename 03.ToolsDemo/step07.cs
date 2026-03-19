#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 7: Chat interactivo con herramientas
using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Streaming = true,
    Tools =
    [
        AIFunctionFactory.Create(GetWeather, "get_weather"),
        AIFunctionFactory.Create(GetTime, "get_time"),
    ]
});

Console.WriteLine("Herramientas: get_weather, get_time");
Console.WriteLine("Escribe mensajes (vacio para salir):\n");
while (true)
{
    Console.Write("Tu: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    var done = new TaskCompletionSource<bool>();
    session.On(evt =>
    {
        if (evt is AssistantMessageDeltaEvent d) Console.Write(d.Data.DeltaContent);
        if (evt is SessionIdleEvent) done.TrySetResult(true);
    });

    Console.Write("IA: ");
    await session.SendAsync(new MessageOptions { Prompt = input });
    await done.Task.WaitAsync(TimeSpan.FromMinutes(2));
    Console.WriteLine("\n");
}

await client.StopAsync();
await client.DisposeAsync();

[Description("Gets the current weather for a city")]
static string GetWeather([Description("City name")] string city)
    => $"Weather in {city}: 22C, partly cloudy, humidity 65%";

[Description("Gets the current time for a city/timezone")]
static string GetTime([Description("City name")] string city)
    => $"Current time in {city}: {DateTime.UtcNow:HH:mm} UTC";
