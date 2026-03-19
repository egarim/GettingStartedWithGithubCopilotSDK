// Estado de autenticacion
var auth = await client.GetAuthStatusAsync();
PrintProp("Autenticado:", auth.IsAuthenticated);
PrintProp("Tipo:", auth.AuthType ?? "");
PrintProp("Mensaje:", auth.StatusMessage ?? "");
return auth.IsAuthenticated;