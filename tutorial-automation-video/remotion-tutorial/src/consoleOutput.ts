export const consoleOutputs: string[] = [
  // code1 - Estructura base
  `$ dotnet run
01 - DEMO: Ciclo de vida y conexion del cliente`,

  // code2 - La clase ClientDemo
  `$ dotnet run
================================================================
  01 - DEMO: Ciclo de vida y conexion del cliente
================================================================`,

  // code3 - Crear el cliente
  `=== 1. Creando CopilotClient (UseLoggedInUser = true) ===
  Estado inicial:        Disconnected`,

  // code4 - Iniciar el cliente
  `=== 2. Iniciando cliente (StartAsync) ===
  Estado tras iniciar:   Connected`,

  // code5 - Ping
  `=== 3. Ping ===
  Enviado:               "hello from demo!"
  Respuesta:             "pong: hello from demo!"
  Timestamp:             1773487538485`,

  // code6 - GetStatus
  `=== 4. Estado (GetStatusAsync) ===
  Version:               0.0.403
  Version Protocolo:     2`,

  // code7 - GetAuthStatus
  `=== 5. Estado de autenticacion (GetAuthStatusAsync) ===
  Autenticado:           True
  Tipo:                  user
  Mensaje:               ardilla91ai`,

  // code8 - ListModels
  `=== 6. Listar modelos disponibles (ListModelsAsync) ===
  Se encontraron 13 modelo(s):
  claude-sonnet-4.5             Claude Sonnet 4.5
  claude-opus-4.5               Claude Opus 4.5
  gpt-5.2                       GPT-5.2
  gemini-3-pro-preview          Gemini 3 Pro
  ...`,

  // code9 - Parada ordenada
  `=== 7. Parada ordenada (StopAsync) ===
  Estado tras parar:     Disconnected

  Demo completado.`,
];
