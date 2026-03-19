// Paso 5: Estado de autenticacion
PrintStep(5, "Estado de autenticacion (GetAuthStatusAsync)");
var auth = await client.GetAuthStatusAsync();
PrintProp("Autenticado:", auth.IsAuthenticated);
PrintProp("Tipo:", auth.AuthType ?? "");
PrintProp("Mensaje:", auth.StatusMessage ?? "");