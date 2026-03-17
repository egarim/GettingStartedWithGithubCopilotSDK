// ...
Console.WriteLine("  Prompt: Tell me a very short joke (2 sentences max).");
Console.Write("  Streaming: ");
var sb = new StringBuilder();
var idleTcs = new TaskCompletionSource<bool>();