import type { SlideContent } from "./extract-slides.js";

/**
 * Console output keyed by step comment (matched against slide.comment).
 * This approach is position-independent — doesn't break when slides are added/removed.
 */
const OUTPUTS: Record<string, Record<string, string>> = {
  "01": {
    "Estructura base": `$ dotnet run
================================================================
  01 - DEMO: Ciclo de vida y conexion del cliente
================================================================`,
    "Helpers": `================================================================
  01 - DEMO: Ciclo de vida y conexion del cliente
================================================================`,
    "Crear el cliente": `=== 1. Creando CopilotClient (UseLoggedInUser = true) ===
  Estado inicial:        Disconnected`,
    "Iniciar el cliente": `=== 2. Iniciando cliente (StartAsync) ===
  Estado tras iniciar:   Connected`,
    "Ping": `=== 3. Ping ===
  Enviado:               "hello from demo!"
  Respuesta:             "pong: hello from demo!"
  Timestamp:             1773687538485`,
    "Status del servidor": `=== 4. Estado (GetStatusAsync) ===
  Version:               0.0.403
  Version Protocolo:     2`,
    "Estado de autenticacion": `=== 5. Estado de autenticacion (GetAuthStatusAsync) ===
  Autenticado:           True
  Tipo:                  user
  Mensaje:               ardilla91ai`,
    "Listar modelos": `=== 6. Listar modelos disponibles (ListModelsAsync) ===
  Se encontraron 13 modelo(s):
  claude-sonnet-4.5                   Claude Sonnet 4.5
  claude-haiku-4.5                    Claude Haiku 4.5
  claude-opus-4.5                     Claude Opus 4.5
  claude-sonnet-4                     Claude Sonnet 4
  gemini-3-pro-preview                Gemini 3 Pro (Preview)
  gpt-5.2                             GPT-5.2
  gpt-5.1                             GPT-5.1
  gpt-4.1                             GPT-4.1
  ...`,
    "Parada ordenada": `=== 7. Parada ordenada (StopAsync) ===
  Estado tras parar:     Disconnected

  Demo completado.`,
  },

  "02": {
    "Estructura base": `================================================================
  02 - DEMO: Ciclo de vida, eventos y multi-turno
================================================================

  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Crear y destruir una sesion": `=== 1. Crear y destruir una sesion ===
  Sesion creada:         cf7bb019-...
  Mensajes iniciales:    1
  Tipo primer evento:    SystemMessageEvent
  Sesion destruida.
  Error esperado:        Cannot read or write...`,
    "Conversacion con estado multi-turno": `=== 2. Conversacion con estado multi-turno ===
  Q1: What is 10 + 15?
  A1: 10 + 15 = 25
  Q2: Now double that result.
  A2: 25 × 2 = 50
  (El modelo recuerda la respuesta anterior)`,
    "Suscripcion a eventos": `=== 3. Suscripcion a eventos (session.On) ===
  Prompt: What is 100 + 200?
  Eventos recibidos:
    • AssistantMessageDeltaEvent
    • AssistantMessageEvent
    • SessionIdleEvent`,
    "SendAsync (disparar y olvidar)": `=== 4. SendAsync (disparar y olvidar) ===
  Prompt: What is 2+2?
  Despues de SendAsync -> session.idle? False
  Despues de esperar -> session.idle? True
  Respuesta: 2 + 2 = 4`,
    "SendAndWaitAsync (bloquea hasta idle)": `=== 5. SendAndWaitAsync (bloquea hasta idle) ===
  Respuesta: 3 + 3 = 6
  Eventos: assistant.message.delta, assistant.message, session.idle`,
    "Reanudar sesion": `=== 6. Reanudar sesion ===
  Sesion 1: I'll remember the number 42!
  ID coincide:           True
  Mensajes tras reanudar: 4
  Respuesta sesion 2: The number is 42.`,
    "Reanudar sesion inexistente": `=== 7. Reanudar sesion inexistente ===
  Error esperado:        Session not found`,
    "Mensaje de sistema - Modo Append": `=== 8. Mensaje de sistema - Modo Append ===
  Respuesta: I'm GitHub Copilot. Have a nice day!
  Contiene 'Have a nice day!': True`,
    "Mensaje de sistema - Modo Replace": `=== 9. Mensaje de sistema - Modo Replace ===
  Respuesta: I'm Testy McTestface.`,
    "Deltas en streaming": `=== 10. Deltas en streaming ===
  Why don't scientists trust atoms?
  Because they make up everything!
  Caracteres transmitidos: 66`,
  },

  "03": {
    "Estructura base": `================================================================
  03 - DEMO: Herramientas personalizadas (AIFunction)
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Herramienta personalizada simple": `=== 1. Herramienta personalizada simple ===
  Prompt: Use encrypt_string to encrypt: Hello World
  Respuesta: "HELLO WORLD"`,
    "Multiples herramientas": `=== 2. Multiples herramientas ===
    [Tool:get_weather] city=Madrid
    [Tool:get_time] city=Madrid
  Respuesta: 22°C, partly cloudy. Time: 17:23 UTC.`,
    "Tipos complejos de entrada/salida": `=== 3. Tipos complejos (records, arrays) ===
    [Tool called] Table=cities, IDs=[12,19], Sort=True
  Respuesta: Passos (135460), San Lorenzo (204356)
  ToolInvocation.SessionId coincide: True`,
    "Manejo de errores en herramientas": `=== 4. Manejo de errores ===
  Herramienta lanzo excepcion con 'Melbourne'
  Respuesta: Location unknown.
  Contiene 'Melbourne': False`,
    "Filtros AvailableTools y ExcludedTools": `=== 5. Filtros AvailableTools/ExcludedTools ===
  AvailableTools = ["view", "edit"]
  Respuesta: I have view and edit tools.

  ExcludedTools = ["view"]
  Respuesta: I have edit, bash... (view excluded).`,
  },

  "04": {
    "Estructura base": `================================================================
  04 - DEMO: Hooks Pre/Post uso de herramientas
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "PreToolUse Hook (Permitir)": `=== 1. Hook Pre-uso - Permitir ===
    [PreToolUse] Tool: lookup_price -> ALLOW
    [Tool:lookup_price] productName=Widget Pro
  Respuesta: Widget Pro costs $29.99.
  Hooks disparados: 1`,
    "PostToolUse Hook": `=== 2. Hook Post-uso ===
    [Tool:lookup_price] productName=Gadget X
    [PostToolUse] Result: Gadget X, Price: $49.95
  Respuesta: Gadget X costs $49.95.`,
    "Ambos hooks juntos": `=== 3. Ambos hooks Pre y Post juntos ===
    [PRE]  -> lookup_price
    [Tool:lookup_price] Super Deluxe Widget
    [POST] <- lookup_price
  Respuesta: $199.00
  Misma herramienta en ambos: True`,
    "Denegar ejecucion": `=== 4. Denegar ejecucion via PreToolUse ===
    [PreToolUse] DENEGANDO: lookup_price
  Respuesta: Unable to look up the price.
  Herramientas denegadas: 1`,
  },

  "05": {
    "Estructura base": `================================================================
  05 - DEMO: Manejo de solicitudes de permisos
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Aprobar permiso": `=== 1. Aprobar permiso ===
    [Permission] Kind: write -> APROBADO
  Respuesta: File updated.
  Contenido despues: "modified content"`,
    "Denegar permiso": `=== 2. Denegar permiso ===
    [Permission] Kind: write -> DENEGADO
  Contenido (intacto): "protected content"
  Archivo protegido: True`,
    "Comportamiento por defecto": `=== 3. Comportamiento por defecto ===
  Respuesta: 2 + 2 = 4
  (Funciona sin handler)`,
    "Handler asincrono": `=== 4. Handler asincrono ===
    [Permission] Kind: execute -> Aprobado tras espera
  Respuesta: hello from async demo`,
    "ToolCallId": `=== 5. ToolCallId ===
    [Permission] ToolCallId: call_xyz789
  ToolCallIds recibidos: 1`,
    "Error en handler": `=== 6. Error en handler ===
    [Permission] A punto de LANZAR excepcion!
  (SDK deniega automaticamente)`,
    "Permisos al reanudar sesion": `=== 7. Permisos al reanudar sesion ===
    [Permission on Resume] Kind: execute -> APROBADO
  Handler disparado al reanudar: True`,
  },

  "06": {
    "Estructura base": `================================================================
  06 - DEMO: Solicitudes de entrada del usuario
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Entrada con opciones": `=== 1. Entrada con opciones ===
    [AskUser] Opciones: [Option A, Option B]
    [AskUser] Auto-seleccionando: Option A
  Respuesta: You chose Option A!`,
    "Verificar opciones": `=== 2. Verificar opciones ===
    [AskUser] [1] Red  [2] Blue
  Respuesta: You selected Red.
  Con opciones: 1`,
    "Entrada libre": `=== 3. Entrada libre (WasFreeform) ===
    [AskUser] Respuesta libre: "emerald green"
  Respuesta: Your favorite color is emerald green!`,
  },

  "07": {
    "Estructura base": `================================================================
  07 - DEMO: Sesiones infinitas y compactacion
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Compactacion activada": `=== 1. Compactacion activada ===
  Enviando mensaje 1/3: Historia de un dragon...
  Respuesta 1 longitud: 322 chars
    * [CompactacionInicio] Compactacion activada!
  Respuesta 2 longitud: 496 chars
    OK [CompactacionCompleta] Exito: True
  CompactionStart: 1  CompactionComplete: 1`,
    "Compactacion desactivada": `=== 2. Compactacion desactivada ===
  Respuesta: 2+2 = 4
  Eventos disparados: 0
  (Esperado: 0 - compactacion desactivada)`,
  },

  "08": {
    "Estructura base": `================================================================
  08 - DEMO: Carga y configuracion de habilidades
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Cargar y aplicar skill": `=== 1. Cargar y aplicar skill ===
  Marcador: "PINEAPPLE_COCONUT_42"
  Respuesta: Hello! PINEAPPLE_COCONUT_42
  Contiene marcador: True
  OK Skill aplicado exitosamente!`,
    "Desactivar skill": `=== 2. Desactivar skill ===
  DisabledSkills: ["demo-skill"]
  Respuesta: Hello! How can I help?
  Contiene marcador: False
  OK Skill desactivado!`,
    "Sin skill (linea base)": `=== 3. Sin skill (linea base) ===
  Respuesta: Hello there!
  Contiene marcador: False`,
  },

  "09": {
    "Estructura base": `================================================================
  09 - DEMO: Servidores MCP y agentes personalizados
================================================================
  Cliente iniciado.`,
    "Helpers": `  Cliente iniciado.`,
    "Servidor MCP simple": `=== 1. Servidor MCP ===
  McpServers: { "test-server": { Type: "local" } }
  Sesion creada. Prompt: What is 2+2?
  Respuesta: 4`,
    "Multiples servidores MCP": `=== 2. Multiples servidores MCP ===
  Servidores: filesystem-server, database-server`,
    "Agente personalizado": `=== 3. Agente personalizado ===
  Name: "business-analyst", Infer: true
  Respuesta: 10`,
    "Agente con herramientas": `=== 4. Agente con herramientas ===
  Agente: devops-agent
  Herramientas: ["bash", "edit"]`,
    "Agente con MCP propio": `=== 5. Agente con MCP propio ===
  Agente: data-agent
  McpServers: { "agent-db-server" }`,
    "Multiples agentes": `=== 6. Multiples agentes ===
  frontend-agent (Infer: true)
  backend-agent (Infer: False)`,
    "Combinacion MCP + Agentes": `=== 7. Combinacion: MCP + Agentes ===
  McpServers + CustomAgents en misma sesion
  Respuesta: 7 + 7 = 14`,
    "MCP y Agentes al reanudar": `=== 8. MCP y Agentes al reanudar ===
  Sesion reanudada con nuevos MCP y agentes
  Respuesta: 6`,
  },

  "11": {
    "Estructura base": `================================================================
  11 - DEMO: Bring Your Own Key (BYOK)
================================================================`,
    "Helpers": `  Auth: True (user)  Fuente: VS Code login`,
    "Listar modelos": `=== 2. Listar modelos (built-in + BYOK) ===
  Total modelos: 13
  claude-sonnet-4.5, claude-haiku-4.5, claude-opus-4.5
  claude-sonnet-4, gemini-3-pro-preview
  gpt-5.2, gpt-5.1, gpt-4.1 ...`,
    "Chat con modelo por defecto": `=== 3. Chat con modelo por defecto ===
  R: I'm GitHub Copilot, powered by OpenAI.`,
    "Chat con modelo especifico": `=== 4. Chat con modelo especifico ===
  Modelo: claude-sonnet-4
  R: I'm Claude, made by Anthropic.`,
    "Comparar modelos": `=== 5. Comparar modelos ===
  claude-sonnet-4.5  -> Canberra is the capital.
  gpt-5.2            -> The capital is Canberra.
  gpt-4.1            -> Australia's capital is Canberra.`,
    "Modelo + herramientas": `=== 6. Modelo + herramientas ===
  [Tool:get_weather] city=Tokyo
  [Tool:get_time] city=Tokyo
  R: 22°C, partly cloudy. Time: 02:30 JST.`,
    "Streaming": `=== 7. Streaming con modelo custom ===
  Why don't scientists trust atoms?
  Because they make up everything!
  Total chars: 65`,
  },
};

/**
 * Get console outputs for slides, matched by comment keyword.
 * Sub-slides carry forward the parent's console output.
 */
export function getConsoleOutputs(
  demoNumber: string,
  slideCount: number,
  slides?: SlideContent[]
): string[] {
  const map = OUTPUTS[demoNumber] ?? {};
  const result: string[] = [];

  if (slides) {
    // Match by slide comment
    for (const slide of slides) {
      if (slide.type === "title" || slide.type === "ending") {
        result.push("");
        continue;
      }

      // Try exact match
      const baseComment = slide.comment.replace(/\s*\(cont\.\)$/, "");
      const output = map[slide.comment] ?? map[baseComment] ?? "";
      result.push(output);
    }
  }

  // Pad if needed
  while (result.length < slideCount) {
    result.push("");
  }

  // Fill empty code slides with parent's output (carry forward)
  let lastOutput = "";
  for (let i = 0; i < result.length; i++) {
    if (result[i] && result[i].trim().length > 0) {
      lastOutput = result[i];
    } else if (lastOutput && i > 0 && i < result.length - 1) {
      result[i] = lastOutput;
    }
  }

  return result.slice(0, slideCount);
}
