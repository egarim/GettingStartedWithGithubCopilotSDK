// Paso 4: Estado del servidor
PrintStep(4, "Estado (GetStatusAsync)");
var s = await client.GetStatusAsync();
PrintProp("Version:", s.Version);
PrintProp("Version Protocolo:", s.ProtocolVersion);