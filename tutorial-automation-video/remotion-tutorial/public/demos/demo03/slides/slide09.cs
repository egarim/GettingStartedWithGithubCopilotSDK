// Manejo de errores en herramientas
var failingTool = AIFunctionFactory.Create(
    () => { throw new Exception("Secret Internal Error — Melbourne"); },
    "get_user_location",
    "Gets the user's location");