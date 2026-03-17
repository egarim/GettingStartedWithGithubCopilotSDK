// ...
await session.SendAsync(
    new MessageOptions { Prompt = "Tell me a very short joke (2 sentences max)." });
await idleTcs.Task.WaitAsync(TimeSpan.FromMinutes(1));
Console.WriteLine($"\n  Total chars: {sb.Length}");