import { existsSync, mkdirSync, statSync } from "fs";
import { join } from "path";
import { EdgeTTS } from "node-edge-tts";
import type { SlideContent } from "./extract-slides.js";

const FPS = 30;
const PADDING_SECONDS = 2;

const VOICE = "es-MX-DaliaNeural";
const RATE = "+5%";

export interface AudioResult {
  slideNumber: number;
  audioFile: string;
  durationMs: number;
  durationFrames: number;
}

/**
 * Narration explanations per step comment.
 * Maps step comment keywords to explanatory narration in Spanish.
 * These EXPLAIN what the code does, not read variable names.
 */
const NARRATION_MAP: Record<string, string> = {
  // Demo 01
  "Estructura base":
    "Empezamos con la estructura base. Importamos el SDK de Copilot y configuramos el sistema de logging para ver mensajes importantes.",
  Helpers:
    "Aqui definimos las funciones auxiliares. Creamos el cliente con las opciones de autenticacion y funciones para mostrar resultados en la consola.",
  "Crear el cliente":
    "Creamos una instancia del cliente de Copilot. En este momento el cliente existe pero aun no esta conectado al servidor.",
  "Iniciar el cliente":
    "Iniciamos la conexion con el servidor de Copilot. El estado cambia de desconectado a conectado, listo para recibir comandos.",
  Ping: "Enviamos un ping al servidor para verificar que la conexion funciona. El servidor responde con un pong confirmando la comunicacion.",
  "Status del servidor":
    "Consultamos el estado del servidor para obtener la version y el protocolo que esta usando.",
  "Estado de autenticacion":
    "Verificamos el estado de autenticacion. Podemos ver si el usuario esta autenticado, el tipo de cuenta y el nombre de usuario.",
  "Listar modelos":
    "Listamos todos los modelos de inteligencia artificial disponibles. Aqui podemos ver modelos de Claude, GPT, Gemini y otros.",
  "Parada ordenada":
    "Finalmente, detenemos el cliente de forma ordenada. El estado regresa a desconectado y liberamos los recursos.",

  // Demo 02
  "Crear y destruir una sesion":
    "Creamos una sesion de chat y verificamos su estado inicial. Luego la destruimos y confirmamos que ya no se puede usar.",
  "Conversacion con estado multi-turno":
    "Iniciamos una conversacion de multiples turnos. El modelo recuerda las respuestas anteriores, asi podemos hacer preguntas de seguimiento.",
  "Suscripcion a eventos":
    "Nos suscribimos a los eventos de la sesion para monitorear todo lo que ocurre: mensajes, deltas y cuando el modelo termina de responder.",
  "SendAsync (disparar y olvidar)":
    "Enviamos un mensaje con send async, que retorna inmediatamente sin esperar la respuesta. Util cuando no necesitamos bloquear la ejecucion.",
  "SendAndWaitAsync (bloquea hasta idle)":
    "Enviamos un mensaje con send and wait, que bloquea hasta recibir la respuesta completa. Es la forma mas simple de obtener una respuesta.",
  "Reanudar sesion":
    "Reanudamos una sesion anterior usando su identificador. El modelo conserva toda la conversacion previa, incluyendo lo que le pedimos recordar.",
  "Reanudar sesion inexistente":
    "Intentamos reanudar una sesion que no existe. El SDK lanza un error controlado que podemos manejar en nuestro codigo.",
  "Mensaje de sistema - Modo Append":
    "Agregamos un mensaje de sistema en modo append. Esto añade instrucciones al prompt del sistema sin reemplazarlo.",
  "Mensaje de sistema - Modo Replace":
    "Reemplazamos completamente el mensaje de sistema. Ahora el modelo responde con la identidad personalizada que definimos.",
  "Deltas en streaming":
    "Activamos el streaming para recibir la respuesta caracter por caracter, en tiempo real, a medida que el modelo la genera.",
  "Modo interactivo":
    "El modo interactivo permite chatear libremente con el modelo directamente desde la terminal.",

  // Demo 03
  "Herramienta personalizada simple":
    "Definimos una herramienta simple que el modelo puede invocar. En este caso, una funcion que encripta texto convirtiendolo a mayusculas.",
  "Multiples herramientas":
    "Registramos varias herramientas en la sesion. El modelo decide automaticamente cual usar segun la pregunta del usuario.",
  "Tipos complejos de entrada/salida":
    "Creamos herramientas que reciben y devuelven tipos complejos como records y arrays. El SDK serializa automaticamente los datos.",
  "Manejo de errores en herramientas":
    "Demostramos que cuando una herramienta falla, el SDK protege la informacion sensible. El modelo no ve los detalles del error.",
  "Filtros AvailableTools y ExcludedTools":
    "Controlamos que herramientas estan disponibles para el modelo. Podemos permitir solo algunas o excluir las que no queremos.",

  // Demo 04
  "PreToolUse Hook (Permitir)":
    "Registramos un hook que se ejecuta antes de cada herramienta. Aqui decidimos permitir la ejecucion y registramos la llamada.",
  "PostToolUse Hook":
    "El hook post-uso nos permite inspeccionar el resultado de la herramienta despues de ejecutarse, util para auditoria.",
  "Ambos hooks juntos":
    "Combinamos ambos hooks en la misma sesion. Primero se ejecuta el pre, luego la herramienta, y finalmente el post.",
  "Denegar ejecucion":
    "Demostramos como bloquear una herramienta desde el hook. El modelo recibe un aviso de que no pudo acceder a la funcion.",

  // Demo 05
  "Aprobar permiso":
    "Configuramos un handler que aprueba automaticamente solicitudes de escritura. El modelo puede editar archivos con nuestro permiso.",
  "Denegar permiso":
    "Ahora denegamos los permisos. El archivo queda protegido porque nuestro handler bloquea la operacion de escritura.",
  "Comportamiento por defecto":
    "Sin un handler configurado, las operaciones basicas de chat funcionan normalmente. Los permisos solo se piden para escritura y ejecucion.",
  "Handler asincrono":
    "El handler de permisos puede ser asincrono. Aqui simulamos una verificacion que toma tiempo antes de aprobar.",
  ToolCallId:
    "Cada solicitud de permiso incluye un identificador unico de la llamada. Util para rastrear y auditar las operaciones.",
  "Error en handler":
    "Si el handler de permisos falla, el SDK lo maneja elegantemente. El permiso se deniega automaticamente sin interrumpir la sesion.",
  "Permisos al reanudar sesion":
    "Al reanudar una sesion existente, podemos registrar un nuevo handler de permisos que se activa en la sesion recuperada.",

  // Demo 06
  "Entrada con opciones":
    "Configuramos un handler que responde automaticamente cuando el modelo necesita entrada del usuario, seleccionando la primera opcion disponible.",
  "Verificar opciones":
    "Verificamos que las opciones presentadas por el modelo coinciden con lo esperado. Las opciones se enumeran para que el usuario elija.",
  "Entrada libre":
    "El modelo puede pedir texto libre, sin opciones predefinidas. Respondemos con una respuesta personalizada que el modelo incorpora.",

  // Demo 07
  "Compactacion activada":
    "Activamos la compactacion con umbrales bajos para que se active rapido. Enviamos mensajes largos para llenar el contexto y disparar la compactacion.",
  "Compactacion desactivada":
    "Comparamos con la compactacion desactivada. Sin ella, no se generan eventos de compactacion, confirmando que el control funciona.",

  // Demo 08
  "Cargar y aplicar skill":
    "Cargamos un skill desde un archivo que instruye al modelo a incluir un marcador especifico. Verificamos que el marcador aparece en la respuesta.",
  "Desactivar skill":
    "Desactivamos el skill por nombre. El modelo ya no sigue las instrucciones del skill y el marcador desaparece de las respuestas.",
  "Sin skill (linea base)":
    "Como comparacion, creamos una sesion sin ningun skill. El modelo responde normalmente sin incluir el marcador.",

  // Demo 09
  "Servidor MCP simple":
    "Configuramos un servidor MCP local. El protocolo MCP permite conectar herramientas externas a la sesion de Copilot.",
  "Multiples servidores MCP":
    "Podemos agregar multiples servidores MCP a una sesion, cada uno proporcionando diferentes herramientas y capacidades.",
  "Agente personalizado":
    "Creamos un agente personalizado con un prompt especifico. El modelo puede delegarle tareas segun su especializacion.",
  "Agente con herramientas":
    "Cada agente puede tener acceso restringido a herramientas especificas, limitando lo que puede hacer dentro de la sesion.",
  "Agente con MCP propio":
    "Un agente puede tener sus propios servidores MCP aislados, separados del resto de la sesion.",
  "Multiples agentes":
    "Configuramos varios agentes especializados en la misma sesion. Cada uno tiene su propio perfil y capacidades.",
  "Combinacion MCP + Agentes":
    "Combinamos servidores MCP compartidos con agentes personalizados en la misma sesion, una configuracion avanzada pero muy poderosa.",
  "MCP y Agentes al reanudar":
    "Al reanudar una sesion, podemos agregar nuevos servidores MCP y agentes que no existian en la sesion original.",

  // Demo 11
  "Listar todos los modelos":
    "Listamos todos los modelos disponibles, incluyendo los modelos personalizados BYOK que el administrador haya configurado.",
  "Chat con modelo por defecto":
    "Enviamos un mensaje usando el modelo por defecto. El modelo se identifica al responder.",
  "Chat con modelo especifico":
    "Elegimos un modelo especifico para la sesion. Podemos usar cualquier modelo de la lista, incluyendo los BYOK.",
  "Comparar modelos":
    "Hacemos la misma pregunta a diferentes modelos y comparamos sus respuestas. Cada uno tiene su propio estilo.",
  "Modelo + herramientas":
    "Las herramientas personalizadas funcionan con cualquier modelo, incluyendo los modelos BYOK.",
  Streaming:
    "El streaming funciona igual con modelos personalizados. Recibimos la respuesta caracter por caracter en tiempo real.",
};

/**
 * Generate explanatory narration for a slide.
 */
export function generateNarrationScript(
  slide: SlideContent,
  demoTitle: string
): string {
  if (slide.type === "title" || slide.type === "ending") {
    return slide.comment;
  }

  // Check for exact match first
  if (NARRATION_MAP[slide.comment]) {
    return NARRATION_MAP[slide.comment];
  }

  // Check for partial match (for sub-slides like "Crear el cliente (cont.)")
  const baseComment = slide.comment.replace(/\s*\(cont\.\)$/, "");
  if (slide.isSubSlide && NARRATION_MAP[baseComment]) {
    return "Continuamos con " + baseComment.toLowerCase() + ".";
  }

  // Fallback: generate something from the comment
  return slide.comment + ".";
}

/**
 * Generate audio files using node-edge-tts.
 */
export async function generateAudio(
  slides: SlideContent[],
  demoTitle: string,
  outputDir: string,
  narrationOverrides?: Record<number, string>
): Promise<AudioResult[]> {
  const audioDir = join(outputDir, "audio");
  mkdirSync(audioDir, { recursive: true });

  const tts = new EdgeTTS({ voice: VOICE, rate: RATE });
  const results: AudioResult[] = [];

  for (const slide of slides) {
    const audioFile = `slide${String(slide.slideNumber).padStart(2, "0")}.mp3`;
    const audioPath = join(audioDir, audioFile);

    const text =
      narrationOverrides?.[slide.slideNumber] ??
      generateNarrationScript(slide, demoTitle);

    if (!existsSync(audioPath)) {
      console.log(
        `  TTS slide ${slide.slideNumber}: "${text.slice(0, 70)}..."`
      );

      try {
        await tts.ttsPromise(text, audioPath);
      } catch (err) {
        console.error(
          `  Warning: TTS failed for slide ${slide.slideNumber}:`,
          (err as Error).message
        );
        results.push({
          slideNumber: slide.slideNumber,
          audioFile,
          durationMs: 5000,
          durationFrames: 5 * FPS + PADDING_SECONDS * FPS,
        });
        continue;
      }
    } else {
      console.log(`  Audio exists for slide ${slide.slideNumber}, skipping`);
    }

    const durationMs = await getAudioDuration(audioPath);
    const durationFrames =
      Math.ceil((durationMs / 1000) * FPS) + PADDING_SECONDS * FPS;

    results.push({
      slideNumber: slide.slideNumber,
      audioFile,
      durationMs,
      durationFrames,
    });
  }

  return results;
}

async function getAudioDuration(filePath: string): Promise<number> {
  try {
    const { parseFile } = await import("music-metadata");
    const metadata = await parseFile(filePath);
    return (metadata.format.duration ?? 5) * 1000;
  } catch {
    try {
      const stats = statSync(filePath);
      return (stats.size / 16000) * 1000;
    } catch {
      return 5000;
    }
  }
}
