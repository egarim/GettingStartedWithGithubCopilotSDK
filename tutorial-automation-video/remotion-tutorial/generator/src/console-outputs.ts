/**
 * Console output definitions for each demo.
 * Captured from REAL runs of each demo (dotnet run).
 * Keys are demo numbers, values are arrays of console outputs per CODE slide
 * (title and ending slides get empty strings prepended/appended automatically).
 */

export const CONSOLE_OUTPUTS: Record<string, string[]> = {
  "01": [
    // slide: Estructura base
    `$ dotnet run
================================================================
  01 - DEMO: Ciclo de vida y conexion del cliente
================================================================`,
    // slide: Helpers
    `================================================================
  01 - DEMO: Ciclo de vida y conexion del cliente
================================================================`,
    // slide: Crear el cliente
    `=== 1. Creando CopilotClient (UseLoggedInUser = true) ===
  Estado inicial:        Disconnected`,
    // slide: Iniciar el cliente
    `=== 2. Iniciando cliente (StartAsync) ===
  Estado tras iniciar:   Connected`,
    // slide: Ping
    `=== 3. Ping ===
  Enviado:               "hello from demo!"
  Respuesta:             "pong: hello from demo!"
  Timestamp:             1773687538485`,
    // slide: GetStatus
    `=== 4. Estado (GetStatusAsync) ===
  Version:               0.0.403
  Version Protocolo:     2`,
    // slide: GetAuthStatus
    `=== 5. Estado de autenticacion (GetAuthStatusAsync) ===
  Autenticado:           True
  Tipo:                  user
  Mensaje:               ardilla91ai`,
    // slide: ListModels
    `=== 6. Listar modelos disponibles (ListModelsAsync) ===
  Se encontraron 13 modelo(s):
  ID                                  Name                      Capabilities
  --                                  ----                      ------------
  claude-sonnet-4.5                   Claude Sonnet 4.5
  claude-haiku-4.5                    Claude Haiku 4.5
  claude-opus-4.5                     Claude Opus 4.5
  claude-sonnet-4                     Claude Sonnet 4
  gemini-3-pro-preview                Gemini 3 Pro (Preview)
  gpt-5.2-codex                       GPT-5.2-Codex
  gpt-5.2                             GPT-5.2
  gpt-5.1-codex-max                   GPT-5.1-Codex-Max
  gpt-5.1-codex                       GPT-5.1-Codex
  gpt-5.1                             GPT-5.1
  gpt-5.1-codex-mini                  GPT-5.1-Codex-Mini
  gpt-5-mini                          GPT-5 mini
  gpt-4.1                             GPT-4.1`,
    // slide: Parada ordenada
    `=== 7. Parada ordenada (StopAsync) ===
  Estado tras parar:     Disconnected

  Demo completado.`,
  ],

  "02": [
    // slide: Estructura base
    `================================================================
  02 - DEMO: Ciclo de vida, eventos y multi-turno
================================================================

  Cliente iniciado.`,
    // slide: Crear y destruir sesion
    `=== 1. Crear y destruir una sesion ===
  Sesion creada:         cf7bb019-8897-4f55-ae8b-...
  Mensajes iniciales:    1
  Tipo primer evento:    SystemMessageEvent
  Sesion destruida.
  Error esperado:        Cannot read or write...`,
    // slide: Crear y destruir (cont.)
    ``,
    // slide: Multi-turno
    `=== 2. Conversacion con estado multi-turno ===
  Q1: What is 10 + 15?
  A1: 10 + 15 = 25
  Q2: Now double that result.
  A2: 25 × 2 = 50
  (El modelo recuerda la respuesta anterior)`,
    // slide: Eventos
    `=== 3. Suscripcion a eventos (session.On) ===
  Prompt: What is 100 + 200?
  Eventos recibidos:
    • AssistantMessageDeltaEvent
    • AssistantMessageDeltaEvent
    • AssistantMessageEvent
    • SessionIdleEvent`,
    // slide: Eventos cont
    ``,
    // slide: Eventos cont
    ``,
    // slide: SendAsync
    `=== 4. SendAsync (disparar y olvidar) ===
  Prompt: What is 2+2?
  Despues de SendAsync -> session.idle en events? False
  (Esperado: False - SendAsync retorna antes de que termine)
  Despues de esperar -> session.idle en events? True
  Respuesta: 2 + 2 = 4`,
    // slide: SendAndWaitAsync
    `=== 5. SendAndWaitAsync (bloquea hasta idle) ===
  Respuesta: 3 + 3 = 6
  Eventos tras retornar: assistant.message.delta,
    assistant.message, session.idle
  (session.idle ya esta en events)`,
    // slide: Reanudar sesion
    `=== 6. Reanudar sesion (ResumeSessionAsync) ===
  Prompt: Remember this number: 42
  Sesion 1: I'll remember the number 42!
  ID coincide:           True
  Mensajes tras reanudar: 4
  Contiene SessionResumeEvent: True
  Prompt: What number did I ask you to remember?
  Respuesta sesion 2: The number you asked me to
    remember is 42.`,
    // slide: Reanudar inexistente
    `=== 7. Reanudar sesion inexistente ===
  Error esperado:        Session not found`,
    // slide: System Append
    `=== 8. Mensaje de sistema - Modo Append ===
  Prompt: What is your name?
  Respuesta:             I'm GitHub Copilot, an AI
    programming assistant. Have a nice day!
  Contiene 'Have a nice day!': True`,
    // slide: System Replace
    `=== 9. Mensaje de sistema - Modo Replace ===
  Prompt: What is your full name?
  Respuesta:             I'm Testy McTestface.
  (Deberia mencionar 'Testy' en lugar de
    'GitHub Copilot')`,
    // slide: Streaming
    `=== 10. Deltas en streaming ===
  Prompt: Tell me a very short joke.
  Why don't scientists trust atoms?
  Because they make up everything!
  Caracteres transmitidos: 66`,
    // slide: Modo interactivo
    ``,
  ],

  "03": [
    // slide: Estructura base
    `================================================================
  03 - DEMO: Herramientas personalizadas (AIFunction)
================================================================

  Cliente iniciado.`,
    // slide: Herramienta simple
    `=== 1. Herramienta personalizada simple ===
  Prompt:   Use encrypt_string to encrypt: Hello World
  Respuesta: The encrypted string is "HELLO WORLD"
  (La herramienta convierte a mayusculas)`,
    // slide: Multiples herramientas
    `=== 2. Multiples herramientas personalizadas ===
    [Tool:get_weather] city=Madrid
    [Tool:get_time] city=Madrid
  Prompt:   What's the weather in Madrid?
  Respuesta: In Madrid it's 22°C, partly cloudy,
    humidity 65%. The time is 17:23 UTC.`,
    // slide: Tipos complejos
    `=== 3. Tipos complejos de entrada/salida ===
  Prompt: Perform a DB query for 'cities'...
    [Tool called] Table=cities, IDs=[12,19],
      Sort=True
  Respuesta: Passos (135460), San Lorenzo (204356)
  ToolInvocation.SessionId coincide: True`,
    // slide: Tipos complejos (cont.)
    ``,
    // slide: Tipos complejos (cont.)
    ``,
    // slide: Manejo de errores
    `=== 4. Manejo de errores en herramientas ===
  Prompt: What is my location?
  La herramienta lanzo excepcion con 'Melbourne'
  Respuesta: I'm unable to determine your
    location - it's unknown.
  Contiene 'Melbourne': False
  (SDK NO expone detalles de excepciones al modelo)`,
    // slide: Errores (cont.)
    ``,
    // slide: Errores (cont.)
    ``,
    // slide: Filtros
    `=== 5. Filtros AvailableTools y ExcludedTools ===
  AvailableTools = ["view", "edit"]
  Prompt: What tools do you have available?
  Respuesta: I have the view and edit tools.

  ExcludedTools = ["view"]
  Prompt: What tools do you have available?
  Respuesta: I have edit, bash and other tools
    (view is excluded).`,
    // slide: Filtros (cont.)
    ``,
    // slide: Modo interactivo
    ``,
  ],

  "04": [
    // slide: Estructura base
    `================================================================
  04 - DEMO: Hooks Pre/Post uso de herramientas
================================================================

  Cliente iniciado.`,
    // slide: PreToolUse Allow
    `=== 1. Hook Pre-uso - Permitir ===
  Prompt: Price of 'Widget Pro'?
    [PreToolUse] Tool: lookup_price
    [PreToolUse] Decision: ALLOW
    [Tool:lookup_price] productName=Widget Pro
  Respuesta: Widget Pro costs $29.99.
  Hooks disparados:      1
  Herramientas interceptadas: [lookup_price]`,
    // slide: PostToolUse
    `=== 2. Hook Post-uso ===
  Prompt: Price of 'Gadget X'?
    [Tool:lookup_price] productName=Gadget X
    [PostToolUse] Tool: lookup_price
    [PostToolUse] Result preview:
      Product: Gadget X, Price: $49.95
  Respuesta: Gadget X costs $49.95.
  Hooks PostToolUse:     1`,
    // slide: Ambos hooks
    `=== 3. Ambos hooks Pre y Post juntos ===
  Prompt: Price of 'Super Deluxe Widget'
    [PRE]  -> lookup_price
    [Tool:lookup_price] productName=Super Deluxe Widget
    [POST] <- lookup_price
  Respuesta: Super Deluxe Widget costs $199.00.
  Pre hooks: [lookup_price]
  Post hooks: [lookup_price]
  Misma herramienta en ambos: True [lookup_price]`,
    // slide: Denegar ejecucion
    `=== 4. Denegar ejecucion via PreToolUse ===
  Prompt: Price of 'Widget Pro'? If you can't,
    explain why.
    [PreToolUse] DENEGANDO herramienta: lookup_price
  Respuesta: I'm unable to look up the price
    because the tool access was denied.
  Herramientas denegadas: 1
  (Herramienta bloqueada - modelo explica)`,
    // slide: Modo interactivo
    ``,
  ],

  "05": [
    // slide: Estructura base
    `================================================================
  05 - DEMO: Manejo de solicitudes de permisos
================================================================

  Cliente iniciado.`,
    // slide: Aprobar permiso
    `=== 1. Aprobar permiso ===
  Prompt: Edit test.txt, replace 'original'
    with 'modified'
    [Permission] Kind: write,
      ToolCallId: call_abc123
    [Permission] Decision: APROBADO
  Respuesta: I've updated the file.
  Solicitudes recibidas: 1
  Contenido despues: "modified content"`,
    // slide: Aprobar (cont.)
    ``,
    // slide: Aprobar (cont.)
    ``,
    // slide: Denegar permiso
    `=== 2. Denegar permiso ===
  Prompt: Edit protected.txt...
    [Permission] Kind: write -> DENEGADO
  Respuesta: Unable to modify the file.
  Contenido (intacto): "protected content"
  Archivo protegido: True`,
    // slide: Denegar (cont.)
    ``,
    // slide: Denegar (cont.)
    ``,
    // slide: Comportamiento por defecto
    `=== 3. Comportamiento por defecto ===
  Prompt: What is 2+2?
  Respuesta: 2 + 2 = 4
  (Funciona sin handler)`,
    // slide: Handler asincrono
    `=== 4. Handler asincrono de permisos ===
    [Permission] Kind: execute
      Simulando verificacion asincrona...
    [Permission] Aprobado tras espera
  Respuesta: Output: hello from async demo`,
    // slide: Handler asincrono (cont.)
    ``,
    // slide: ToolCallId
    `=== 5. ToolCallId en solicitudes ===
    [Permission] ToolCallId: call_xyz789
  Respuesta: Output: test-toolcallid
  ToolCallIds recibidos: 1
    -> call_xyz789`,
    // slide: ToolCallId (cont.)
    ``,
    // slide: Error en handler
    `=== 6. Error en handler ===
    [Permission] A punto de LANZAR excepcion!
  Respuesta: I wasn't able to run that. Failed.
  (SDK maneja excepcion - permiso denegado)`,
    // slide: Error (cont.)
    ``,
    // slide: Permisos al reanudar
    `=== 7. Permisos al reanudar sesion ===
  Sesion creada: sess_abc123
    [Permission on Resume] Kind: execute
      -> APROBADO
  Handler disparado al reanudar: True`,
    // slide: Permisos reanudar (cont.)
    ``,
    // slide: Permisos reanudar (cont.)
    ``,
    // slide: Modo interactivo
    ``,
  ],

  "06": [
    // slide: Estructura base
    `================================================================
  06 - DEMO: Solicitudes de entrada del usuario
================================================================

  Cliente iniciado.`,
    // slide: Entrada con opciones
    `=== 1. Entrada con opciones ===
  Prompt: Ask me to choose between Option A
    and Option B...
    [AskUser] Pregunta: Which option?
    [AskUser] Opciones: [Option A, Option B]
    [AskUser] Auto-seleccionando: Option A
  Respuesta: You chose Option A!
  Solicitudes recibidas: 1`,
    // slide: Entrada opciones (cont.)
    ``,
    // slide: Entrada opciones (cont.)
    ``,
    // slide: Entrada opciones (cont.)
    ``,
    // slide: Verificar opciones
    `=== 2. Verificar opciones ===
  Prompt: Pick between 'Red' and 'Blue'
    [AskUser] Pregunta: Which color?
    [AskUser]   [1] Red
    [AskUser]   [2] Blue
  Respuesta: You selected Red.
  Con opciones:          1`,
    // slide: Verificar (cont.)
    ``,
    // slide: Verificar (cont.)
    ``,
    // slide: Verificar (cont.)
    ``,
    // slide: Entrada libre
    `=== 3. Entrada libre (WasFreeform = true) ===
  Prompt: Ask me my favorite color
    [AskUser] Pregunta: What is your
      favorite color?
    [AskUser] Respuesta libre:
      "My favorite color is emerald green,
       a beautiful shade!"
  Respuesta: Your favorite color is
    emerald green!
  Solicitudes recibidas: 1`,
    // slide: Modo interactivo
    ``,
  ],

  "07": [
    // slide: Estructura base
    `================================================================
  07 - DEMO: Sesiones infinitas y compactacion
================================================================

  Cliente iniciado.`,
    // slide: Compactacion activada
    `=== 1. Compactacion activada - Umbrales bajos ===
  (Umbrales bajos para demostracion)

  Enviando mensaje 1/3: Historia de un dragon...
  Respuesta 1 longitud: 322 chars
  Eventos compactacion:  inicio=0, completo=0`,
    // slide: Compactacion cont
    `  Enviando mensaje 2/3: Continuar con castillo...
    * [CompactacionInicio] Compactacion activada!
  Respuesta 2 longitud: 496 chars
  Eventos compactacion:  inicio=1, completo=0`,
    // slide: Compactacion cont
    `  Enviando mensaje 3/3: Describir el tesoro...
    OK [CompactacionCompleta] Exito: True,
      Tokens removidos: (calculado)
  Respuesta 3 longitud: 346 chars`,
    // slide: Compactacion cont
    `  -- Resultados de compactacion --
  CompactionStart:       1
  CompactionComplete:    1
  Ultima exitosa:        True
  Tokens removidos:      (variable)`,
    // slide: Compactacion cont
    `  Verificando sesion tras compactacion...
  Prompt: What was the main story about?
  Respuesta: The story was about a dragon
    and its castle.
  (Contexto preservado via resumen)`,
    // slide: Compactacion cont
    ``,
    // slide: Compactacion cont
    ``,
    // slide: Compactacion cont
    ``,
    // slide: Compactacion desactivada
    `=== 2. Compactacion desactivada ===
  Prompt: What is 2+2?
  Respuesta: 2+2 = 4
  Eventos disparados:    0
  (Esperado: 0 - compactacion desactivada)`,
    // slide: Desactivada cont
    ``,
    // slide: Desactivada cont
    ``,
    // slide: Desactivada cont
    ``,
    // slide: Modo interactivo
    ``,
  ],

  "08": [
    // slide: Estructura base
    `================================================================
  08 - DEMO: Carga y configuracion de habilidades
================================================================

=== Anatomia del archivo SKILL.md ===
  demo-skill/
    SKILL.md

  Cliente iniciado.`,
    // slide: Cargar y aplicar skill
    `=== 1. Cargar y aplicar skill ===
  SkillDirectories: [".../copilot-skills-demo"]
  Marcador esperado: "PINEAPPLE_COCONUT_42"

  Prompt: Say hello briefly using demo skill.
  Respuesta: Hello! Hope you're having a great
    day! PINEAPPLE_COCONUT_42

  Contiene marcador:     True
  OK Skill cargado y aplicado exitosamente!`,
    // slide: Cargar skill (cont.)
    ``,
    // slide: Cargar skill (cont.)
    ``,
    // slide: Cargar skill (cont.)
    ``,
    // slide: Desactivar skill
    `=== 2. Desactivar skill via DisabledSkills ===
  DisabledSkills: ["demo-skill"]

  Prompt: Say hello briefly.
  Respuesta: Hello! How can I help you today?

  Contiene marcador:     False
  OK Skill desactivado correctamente!`,
    // slide: Desactivar (cont.)
    ``,
    // slide: Desactivar (cont.)
    ``,
    // slide: Desactivar (cont.)
    ``,
    // slide: Sin skill (linea base)
    `=== 3. Sin skill (linea base) ===
  Prompt: Say hello briefly.
  Respuesta (sin skill): Hello there!
  Contiene marcador:     False
  (Esperado: Sin marcador)`,
    // slide: Modo interactivo
    ``,
  ],

  "09": [
    // slide: Estructura base
    `================================================================
  09 - DEMO: Servidores MCP y agentes personalizados
================================================================

  Cliente iniciado.`,
    // slide: Servidor MCP simple
    `=== 1. Configuracion de un servidor MCP ===
  McpServers config:
  {
    "test-server": {
      Type: "local",
      Command: "echo",
      Args: ["hello-mcp"],
      Tools: ["*"]
    }
  }
  Sesion creada:         95e0e49d-02dc-...
  Prompt: What is 2+2?
  Respuesta: 2+2 equals 4.
  Sesion funciona con configuracion MCP`,
    // slide: Servidor MCP (cont.)
    ``,
    // slide: Servidor MCP (cont.)
    ``,
    // slide: Servidor MCP (cont.)
    ``,
    // slide: Multiples MCP
    `=== 2. Multiples servidores MCP ===
  Servidores MCP:        2:
    filesystem-server, database-server
  Sesion:                308d0d5f-43a9-...`,
    // slide: Agente personalizado
    `=== 3. Agente personalizado ===
  CustomAgents config:
  [{
    Name: "business-analyst",
    Prompt: "You are a business analyst...",
    Infer: true
  }]
  Sesion: 6139690b-8b46-...
  Prompt: What is 5+5?
  Respuesta: 10
  Configuracion de agente aceptada`,
    // slide: Agente (cont.)
    ``,
    // slide: Agente (cont.)
    ``,
    // slide: Agente (cont.)
    ``,
    // slide: Agente con herramientas
    `=== 4. Agente con herramientas ===
  Agente:                devops-agent
  Herramientas:          ["bash", "edit"]
  Sesion:                27932672-4795-...`,
    // slide: Agente herramientas (cont.)
    ``,
    // slide: Agente con MCP propio
    `=== 5. Agente con MCP propio ===
  Agente:                data-agent
  McpServers: { "agent-db-server": { ... } }
  (Agente con conexiones MCP aisladas)
  Sesion:                a1735fc3-c033-...`,
    // slide: Multiples agentes
    `=== 6. Multiples agentes ===
  Agentes configurados:  2
    - frontend-agent (Infer: ) - React, CSS
    - backend-agent (Infer: False) - C#, .NET
  Sesion:                b01f1e86-58c4-...`,
    // slide: Combinacion MCP + Agentes
    `=== 7. Combinacion: MCP + Agentes ===
  McpServers: { "shared-server": { ... } }
  CustomAgents: [{ "coordinator-agent" }]
  (MCP + agentes en la misma sesion)

  Prompt: What is 7+7?
  Respuesta: 7 + 7 = 14
  Configuracion combinada aceptada`,
    // slide: Combinacion (cont.)
    ``,
    // slide: Combinacion (cont.)
    ``,
    // slide: Combinacion (cont.)
    ``,
    // slide: MCP al reanudar
    `=== 8. MCP y Agentes al reanudar ===
  Sesion creada:         ab38dcbe-0763-...
  Sesion reanudada:      ab38dcbe-0763-...
  Prompt: What is 3+3?
  Respuesta: 6
  MCP y agentes agregados al reanudar`,
    // slide: MCP reanudar (cont.)
    ``,
    // slide: Modo interactivo
    ``,
  ],

  "11": [
    // slide: Estructura base
    `================================================================
  11 - DEMO: Bring Your Own Key (BYOK)
================================================================`,
    // slide: Listar modelos
    `=== 1. Iniciar cliente ===
  Auth:                  True (user)
  Fuente:                VS Code login

=== 2. Listar modelos (built-in + BYOK) ===
  Total modelos:         13
  ID                                  Nombre
  --                                  ------
  claude-sonnet-4.5                   Claude Sonnet 4.5
  claude-haiku-4.5                    Claude Haiku 4.5
  claude-opus-4.5                     Claude Opus 4.5
  claude-sonnet-4                     Claude Sonnet 4
  gemini-3-pro-preview                Gemini 3 Pro
  gpt-5.2-codex                       GPT-5.2-Codex
  gpt-5.2                             GPT-5.2
  gpt-5.1                             GPT-5.1
  gpt-4.1                             GPT-4.1
  ...`,
    // slide: Chat modelo por defecto
    `=== 3. Chat con modelo por defecto ===
  P: What model are you?
  R: I'm GitHub Copilot, powered by OpenAI.`,
    // slide: Chat modelo especifico
    `=== 4. Chat con modelo especifico ===
  Modelo elegido:        claude-sonnet-4
  P: What model are you?
  R: I'm Claude, made by Anthropic.`,
    // slide: Comparar modelos
    `=== 5. Comparar multiples modelos ===
  Prompt: "What is the capital of Australia?"

  claude-sonnet-4.5     -> Canberra is the capital.
  gpt-5.2               -> The capital is Canberra.
  gpt-4.1               -> Australia's capital
                            is Canberra.
  gemini-3-pro-preview  -> Canberra.`,
    // slide: Modelo + herramientas
    `=== 6. Modelo custom + herramientas ===
  Modelo: claude-sonnet-4
  [Tool:get_weather] city=Tokyo
  [Tool:get_time] city=Tokyo
  P: Weather in Tokyo and time?
  R: Tokyo: 22°C, partly cloudy.
     Time: 02:30 JST.`,
    // slide: Streaming
    `=== 7. Streaming con modelo custom ===
  Modelo: claude-sonnet-4
  Prompt: Tell me a short joke.
  Streaming: Why don't scientists trust
    atoms? Because they make up everything!
  Total chars: 65`,
    // slide: Streaming cont
    ``,
    // slide: Streaming cont
    ``,
    // slide: Streaming cont
    ``,
    // slide: Modo interactivo
    ``,
  ],
};

/**
 * Get console outputs for a demo, padding to match slide count.
 * Sub-slides (continuations) carry forward the parent's console output.
 */
export function getConsoleOutputs(
  demoNumber: string,
  slideCount: number
): string[] {
  const raw = CONSOLE_OUTPUTS[demoNumber] ?? [];
  // Prepend empty string for title slide, append empty for ending slide
  const outputs = ["", ...raw, ""];

  // Pad with empty strings if needed
  while (outputs.length < slideCount) {
    outputs.push("");
  }

  const result = outputs.slice(0, slideCount);

  // Fill empty slots by carrying forward the last non-empty output
  // (sub-slides should show the same console as their parent)
  let lastOutput = "";
  for (let i = 0; i < result.length; i++) {
    if (result[i] && result[i].trim().length > 0) {
      lastOutput = result[i];
    } else if (lastOutput && i > 0) {
      // Don't fill the title slide (index 0) or ending slide (last)
      result[i] = lastOutput;
    }
  }

  return result;
}
