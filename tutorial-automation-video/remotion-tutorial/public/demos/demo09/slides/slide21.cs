// MCP y Agentes al reanudar
var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
Console.WriteLine("  Prompt: What is 1+1?");
await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What is 1+1?" });
PrintProp("Sesion creada:", sessionId);