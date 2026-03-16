/**
 * Title slide descriptions and narration for each demo.
 */

export interface DemoDescription {
  title: string;
  subtitle: string; // short tagline
  bullets: string[]; // what we'll learn (3-4 items)
  narration: string; // TTS script for title slide
  endNarration: string; // TTS script for ending slide
}

const DEMO_ORDER = ["01", "02", "03", "04", "05", "06", "07", "08", "09", "11"];

export function getNextDemo(demoNumber: string): { number: string; title: string } | null {
  const idx = DEMO_ORDER.indexOf(demoNumber);
  if (idx === -1 || idx === DEMO_ORDER.length - 1) return null;
  const next = DEMO_ORDER[idx + 1];
  return { number: next, title: DESCRIPTIONS[next].title };
}

export const DESCRIPTIONS: Record<string, DemoDescription> = {
  "01": {
    title: "Ciclo de vida del cliente",
    subtitle: "Conectar, consultar y desconectar el CopilotClient",
    bullets: [
      "Crear y configurar CopilotClient",
      "Iniciar conexion con StartAsync",
      "Consultar estado, modelos y ping",
      "Parada ordenada con StopAsync",
    ],
    narration:
      "Demo uno. Ciclo de vida y conexion del cliente. " +
      "En este video aprenderemos a crear el cliente de Copilot, " +
      "conectarnos al servidor, hacer consultas basicas como ping, " +
      "listar modelos, y finalmente cerrar la conexion de forma ordenada.",
    endNarration:
      "Excelente. Ya conoces el ciclo de vida completo del cliente. " +
      "En el siguiente video veremos como crear sesiones y mantener " +
      "conversaciones multi-turno con el modelo.",
  },
  "02": {
    title: "Sesiones y conversaciones",
    subtitle: "Multi-turno, eventos y streaming",
    bullets: [
      "Crear y gestionar sesiones",
      "Conversaciones multi-turno",
      "Suscripcion a eventos y deltas",
      "Reanudar sesiones existentes",
    ],
    narration:
      "Demo dos. Sesiones y conversaciones multi-turno. " +
      "Aprenderemos a crear sesiones, mantener el contexto entre mensajes, " +
      "suscribirnos a eventos en tiempo real y reanudar conversaciones previas.",
    endNarration:
      "Perfecto. Ya sabes como manejar sesiones completas. " +
      "En el siguiente video aprenderemos a registrar herramientas personalizadas " +
      "para que el modelo pueda ejecutar funciones de tu codigo.",
  },
  "03": {
    title: "Tools: funciones personalizadas",
    subtitle: "Registrar y ejecutar herramientas via chat",
    bullets: [
      "Definir herramientas con AIFunction",
      "Registrar tools en la sesion",
      "Invocacion automatica via chat",
      "Tipos complejos y manejo de errores",
    ],
    narration:
      "Demo tres. Tools, funciones personalizadas. " +
      "Veremos como definir herramientas que el modelo puede invocar, " +
      "registrarlas en la sesion, y manejar tipos complejos de entrada y salida.",
    endNarration:
      "Muy bien. Ya puedes crear herramientas personalizadas. " +
      "En el siguiente video veremos los hooks, que te permiten interceptar " +
      "la ejecucion de tools antes y despues de que ocurran.",
  },
  "04": {
    title: "Hooks: pre y post ejecucion",
    subtitle: "Interceptar y controlar la ejecucion de tools",
    bullets: [
      "Hook pre-tool: validar antes de ejecutar",
      "Hook post-tool: procesar resultados",
      "Bloquear ejecuciones no autorizadas",
    ],
    narration:
      "Demo cuatro. Hooks, pre y post ejecucion. " +
      "Aprenderemos a interceptar las llamadas a herramientas " +
      "para validar, registrar o incluso bloquear su ejecucion.",
    endNarration:
      "Excelente. Los hooks te dan control total sobre las herramientas. " +
      "En el siguiente video exploraremos el sistema de permisos y autorizacion.",
  },
  "05": {
    title: "Permisos y autorizacion",
    subtitle: "Control de acceso y flujos de autorizacion",
    bullets: [
      "Verificar permisos de Copilot",
      "Scopes y tokens OAuth",
      "Flujo de autorizacion de dispositivo",
      "Manejar permisos revocados",
    ],
    narration:
      "Demo cinco. Permisos y autorizacion. " +
      "Veremos como verificar el acceso del usuario, " +
      "trabajar con tokens OAuth y manejar escenarios " +
      "donde los permisos son revocados.",
    endNarration:
      "Perfecto. Ya dominas el sistema de permisos. " +
      "En el siguiente video veremos como pedirle informacion al usuario " +
      "directamente desde el flujo de chat.",
  },
  "06": {
    title: "AskUser: interaccion con el usuario",
    subtitle: "Solicitar entrada del usuario durante el chat",
    bullets: [
      "Registrar handlers de AskUser",
      "Opciones de seleccion multiple",
      "Entrada de texto libre",
    ],
    narration:
      "Demo seis. Ask User, interaccion con el usuario. " +
      "Aprenderemos a configurar handlers que permiten al modelo " +
      "solicitar informacion al usuario durante la conversacion.",
    endNarration:
      "Muy bien. Ya sabes como interactuar con el usuario. " +
      "En el siguiente video veremos la compactacion de contexto, " +
      "esencial para conversaciones largas.",
  },
  "07": {
    title: "Compactacion de contexto",
    subtitle: "Optimizar el uso de tokens en conversaciones largas",
    bullets: [
      "Llenar el contexto con muchos mensajes",
      "Activar compactacion automatica",
      "Medir reduccion de tokens",
    ],
    narration:
      "Demo siete. Compactacion de contexto. " +
      "Veremos como el SDK puede comprimir automaticamente el historial " +
      "de conversacion para mantener el uso de tokens bajo control.",
    endNarration:
      "Perfecto. La compactacion es clave para aplicaciones en produccion. " +
      "En el siguiente video exploraremos las skills del SDK.",
  },
  "08": {
    title: "Skills del SDK",
    subtitle: "Habilidades predefinidas para tareas comunes",
    bullets: [
      "Listar skills disponibles",
      "Ejecutar skills como explain y test-gen",
      "Configurar parametros de skills",
    ],
    narration:
      "Demo ocho. Skills del SDK. " +
      "Aprenderemos a usar las habilidades predefinidas del SDK " +
      "como explicacion de codigo, generacion de tests y mas.",
    endNarration:
      "Excelente. Las skills aceleran tareas comunes de desarrollo. " +
      "En el siguiente video veremos MCP Agents, " +
      "una forma poderosa de conectar agentes externos.",
  },
  "09": {
    title: "MCP Agents",
    subtitle: "Conectar agentes externos via Model Context Protocol",
    bullets: [
      "Crear servidores MCP",
      "Registrar capabilities y tools",
      "Cadenas de agentes",
      "Combinar MCP con herramientas locales",
    ],
    narration:
      "Demo nueve. MCP Agents. " +
      "Veremos como conectar agentes externos usando el protocolo MCP, " +
      "registrar sus capacidades y combinarlos con herramientas locales.",
    endNarration:
      "Muy bien. MCP abre un mundo de posibilidades con agentes. " +
      "En el ultimo video veremos como usar modelos personalizados " +
      "con OpenRouter y el patron BYOK.",
  },
  "11": {
    title: "OpenRouter: modelos BYOK",
    subtitle: "Usar modelos personalizados via Bring Your Own Key",
    bullets: [
      "Configurar modelos de OpenRouter",
      "Crear cliente BYOK sin cuenta GitHub",
      "Comparar respuestas entre modelos",
      "Manejar errores de modelo",
    ],
    narration:
      "Demo once. OpenRouter y modelos BYOK. " +
      "Aprenderemos a usar modelos de terceros como Mistral o Llama " +
      "a traves de OpenRouter, sin necesidad de una cuenta de GitHub Copilot.",
    endNarration:
      "Felicidades. Has completado toda la serie del SDK de GitHub Copilot. " +
      "Ahora tienes las herramientas para construir aplicaciones potentes " +
      "con inteligencia artificial integrada. Hasta la proxima!",
  },
};
