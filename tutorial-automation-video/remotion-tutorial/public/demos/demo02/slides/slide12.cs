// Reanudar sesion
var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
Console.WriteLine("  Prompt: Remember this number: 42");
var a1 = await session1.SendAndWaitAsync(new MessageOptions { Prompt = "Remember this number: 42" });
Console.WriteLine($"  Sesion 1 (ID: {sessionId}): {a1?.Data.Content?.Substring(0, Math.Min(100, a1.Data.Content?.Length ?? 0))}");

var session2 = await client.ResumeSessionAsync(sessionId);
PrintProp("ID coincide:", session2.SessionId == sessionId);

var messages = await session2.GetMessagesAsync();
PrintProp("Mensajes tras reanudar:", messages.Count);
Console.WriteLine($"  Contiene SessionResumeEvent: {messages.Any(m => m is SessionResumeEvent)}");

Console.WriteLine("  Prompt: What number did I ask you to remember?");
var a2 = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What number did I ask you to remember?" });
Console.WriteLine($"  Respuesta sesion 2: {a2?.Data.Content?.Substring(0, Math.Min(100, a2.Data.Content?.Length ?? 0))}");

await session2.DisposeAsync();