// ...
Console.WriteLine("  Prompt: Perform a DB query for the 'cities' table using IDs 12 and 19, sorting ascending.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Perform a DB query for the 'cities' table using IDs 12 and 19, sorting ascending. " +
             "Reply only with lines of the form: [cityname] [population]"
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine($"  ToolInvocation.SessionId coincide: {receivedInvocation?.SessionId == session.SessionId}");
await session.DisposeAsync();