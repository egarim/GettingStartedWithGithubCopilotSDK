// Paso 2: Herramienta personalizada simple (encrypt_string)
await client.StartAsync();
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
});
var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Use encrypt_string to encrypt: Hello World" });
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> HELLO WORLD
await session.DisposeAsync();
await client.StopAsync();