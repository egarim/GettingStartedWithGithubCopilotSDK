// Crear y destruir una sesion
var session = await client.CreateSessionAsync(new SessionConfig { Model = "gpt-4o" });
PrintProp("Sesion creada:", session.SessionId);

var messages = await session.GetMessagesAsync();
PrintProp("Mensajes iniciales:", messages.Count);
PrintProp("Tipo primer evento:", messages[0].GetType().Name);