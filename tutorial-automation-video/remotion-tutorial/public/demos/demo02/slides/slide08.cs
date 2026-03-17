// ...
Console.WriteLine("  Prompt: What is 100 + 200?");
await session.SendAsync(new MessageOptions { Prompt = "What is 100 + 200?" });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
sub.Dispose();