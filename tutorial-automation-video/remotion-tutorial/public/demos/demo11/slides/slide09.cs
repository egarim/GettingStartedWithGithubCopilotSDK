// Streaming
PrintProp("Modelo:", chosenModel);
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = chosenModel,
    Streaming = true
});