// Herramienta personalizada simple
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(EncryptString, "encrypt_string")]
});

var answer = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Use encrypt_string to encrypt this string: Hello World" });
Console.WriteLine($"  Prompt:   Use encrypt_string to encrypt: Hello World");
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
Console.WriteLine("  (La herramienta convierte a mayusculas — el modelo deberia incluir 'HELLO WORLD')");
await session.DisposeAsync();