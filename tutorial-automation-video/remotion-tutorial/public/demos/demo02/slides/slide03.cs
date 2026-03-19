// Paso 2: Crear y destruir una sesion
var client = new CopilotClient(new CopilotClientOptions { UseLoggedInUser = true,
    Logger = loggerFactory.CreateLogger<CopilotClient>() });
await client.StartAsync();
var session = await client.CreateSessionAsync(new SessionConfig { Model = "gpt-4o" });
Console.WriteLine($"Sesion creada: {session.SessionId}");
var messages = await session.GetMessagesAsync();
Console.WriteLine($"Mensajes iniciales: {messages.Count}"); // -> 1 (sistema)
await session.DisposeAsync(); // destruir sesion
Console.WriteLine("Sesion destruida.");
await client.StopAsync();