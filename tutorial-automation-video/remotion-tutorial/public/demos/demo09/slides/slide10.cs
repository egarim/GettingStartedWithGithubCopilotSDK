// Paso 9: MCP y Agentes al reanudar sesion (ResumeSessionAsync)
// Crear sesion inicial
var session1 = await client.CreateSessionAsync();
var sessionId = session1.SessionId;
await session1.SendAndWaitAsync(new MessageOptions { Prompt = "What is 1+1?" });
// Reanudar sesion con MCP y agentes adicionales
var session2 = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
        ["resume-server"] = new McpLocalServerConfig
            Type = "local", Command = "echo", Args = ["hello-resume"], Tools = ["*"]
            Name = "resume-agent", DisplayName = "Resume Agent",
            Description = "Added on resume", Prompt = "You are a resume agent."
var answer = await session2.SendAndWaitAsync(new MessageOptions { Prompt = "What is 3+3?" });
Console.WriteLine($"  Respuesta (sesion reanudada): {answer?.Data.Content}");
await session2.DisposeAsync();