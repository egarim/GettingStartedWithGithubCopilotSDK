// ...
Console.WriteLine("  Prompt: Use the ask_user tool to ask me to pick between 'Red' and 'Blue'.");
var answer = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "Use the ask_user tool to ask me to pick between exactly two options: 'Red' and 'Blue'. These should be provided as choices. Wait for my answer."
});
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");