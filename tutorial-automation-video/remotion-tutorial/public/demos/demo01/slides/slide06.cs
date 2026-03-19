// Ping
var pong = await client.PingAsync("hello from demo!");
PrintProp("Enviado:", "\"hello from demo!\"");
PrintProp("Respuesta:", $"\"{pong.Message}\"");
PrintProp("Timestamp:", pong.Timestamp);