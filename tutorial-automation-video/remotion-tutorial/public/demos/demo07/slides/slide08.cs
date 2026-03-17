// ...
Console.WriteLine("  Enviando mensaje 3/3: Describir el tesoro...");
var a3 = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Now describe the dragon's treasure in great detail. List every item. Make this response very long."
});
Console.WriteLine($"  Respuesta 3 longitud: {a3?.Data.Content?.Length ?? 0} chars\n");