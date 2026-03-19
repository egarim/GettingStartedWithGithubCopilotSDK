// Status del servidor
var s = await client.GetStatusAsync();
PrintProp("Version:", s.Version);
PrintProp("Version Protocolo:", s.ProtocolVersion);