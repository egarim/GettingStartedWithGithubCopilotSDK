// Conversacion con estado multi-turno
await using var session = await client.CreateSessionAsync();

var answer1 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 10 + 15?" });
Console.WriteLine($"  Q1: What is 10 + 15?");
Console.WriteLine($"  A1: {answer1?.Data.Content?.Substring(0, Math.Min(150, answer1.Data.Content?.Length ?? 0))}");

var answer2 = await session.SendAndWaitAsync(new MessageOptions { Prompt = "Now double that result." });
Console.WriteLine($"  Q2: Now double that result.");
Console.WriteLine($"  A2: {answer2?.Data.Content?.Substring(0, Math.Min(150, answer2.Data.Content?.Length ?? 0))}");
Console.WriteLine("  (El modelo recuerda la respuesta anterior)");