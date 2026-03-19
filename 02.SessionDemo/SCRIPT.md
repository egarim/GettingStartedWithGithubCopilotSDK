# Demo 02 — Sesiones, eventos y multi-turno

## Intro (slide de titulo)
En esta demo entramos al corazon del SDK: las sesiones. Una sesion es una conversacion con estado entre nuestra aplicacion y el modelo. Vamos a ver como crearlas, enviar mensajes, suscribirnos a eventos, hacer streaming token por token, y hasta reanudar sesiones previas. Tambien veremos como personalizar el comportamiento del modelo con system messages.

## step01.cs — Boilerplate del cliente
Creamos el cliente con la misma estructura de la demo anterior. El logger, las opciones con UseLoggedInUser, y un titulo en consola. Es simplemente el punto de partida para poder crear sesiones.

## step02.cs — Crear y destruir una sesion
Creamos nuestra primera sesion con CreateSessionAsync, especificando el modelo GPT-4o. Al crearla, obtenemos un SessionId unico y podemos verificar que ya tiene un mensaje del sistema por defecto. En consola veran el ID de la sesion y que empieza con 1 mensaje. Al final la destruimos con DisposeAsync.

## step03.cs — Conversacion multi-turno con SendAndWaitAsync
Aqui es donde la magia sucede. Enviamos una pregunta matematica y recibimos la respuesta. Luego enviamos un segundo mensaje que referencia el resultado anterior, y el modelo lo recuerda. En consola veran "25" y luego "50", demostrando que la sesion mantiene el contexto entre turnos.

## step04.cs — Suscripcion a eventos con session.On
En vez de simplemente esperar la respuesta, nos suscribimos a todos los eventos que emite la sesion. Usamos SendAsync (sin Wait) y esperamos manualmente al evento SessionIdleEvent. En consola veran la lista de eventos que se dispararon: AssistantMessageEvent, SessionIdleEvent, etc. Esto nos da visibilidad total del ciclo de vida de un mensaje.

## step05.cs — SendAsync (fire-and-forget)
Demostramos la diferencia clave entre SendAsync y SendAndWaitAsync. Con SendAsync el control regresa inmediatamente, antes de que el modelo termine de responder. En consola veran que "session.idle" no esta en la lista de eventos justo despues de enviar, pero si aparece despues de esperar manualmente con un TaskCompletionSource.

## step06.cs — SendAndWaitAsync (bloqueante hasta idle)
El contraste con el paso anterior: SendAndWaitAsync no retorna hasta que la sesion llega al estado idle. En consola veran que la respuesta ya esta disponible y que "session.idle" ya aparece en los eventos. Para la mayoria de casos de uso, este metodo es mas simple y seguro.

## step07.cs — Reanudar sesion con ResumeSessionAsync
Creamos una sesion, le pedimos que recuerde el numero 42, y luego la reanudamos con ResumeSessionAsync usando el mismo SessionId. Al preguntar por el numero, el modelo lo recuerda perfectamente. En consola veran que los IDs coinciden y que la respuesta incluye "42". Esto es clave para persistencia de conversaciones.

## step08.cs — Reanudar sesion inexistente (manejo de errores)
Que pasa si intentamos reanudar una sesion con un ID inventado? El SDK lanza una IOException. En consola veran el mensaje de error capturado. Es importante manejar este caso en produccion, por ejemplo cuando un usuario vuelve con un ID de sesion expirado.

## step09.cs — Mensaje de sistema en modo Append
Configuramos un SystemMessage con modo Append, que agrega nuestra instruccion al system prompt por defecto de Copilot. Le pedimos que termine cada respuesta con "Have a nice day!". En consola veran que la respuesta del modelo incluye esa frase al final, combinada con el comportamiento normal de Copilot.

## step10.cs — Mensaje de sistema en modo Replace
Ahora usamos modo Replace, que reemplaza completamente el system prompt. Le damos una identidad nueva: "Testy McTestface". Al preguntar su nombre, en consola veran que responde con ese nombre en vez de "GitHub Copilot". La diferencia con Append es total: aqui no queda nada del prompt original.

## step11.cs — Streaming con deltas (AssistantMessageDeltaEvent)
Activamos streaming en la sesion y nos suscribimos a AssistantMessageDeltaEvent. Cada delta contiene un fragmento de texto que imprimimos inmediatamente con Console.Write. En consola veran el chiste aparecer token por token, como en ChatGPT, en vez de esperar la respuesta completa. Al final mostramos cuantos caracteres se transmitieron.

## step12.cs — Chat interactivo en streaming
El gran final de esta demo: un loop de chat interactivo completo. El usuario escribe, el modelo responde en streaming, y el ciclo se repite. Combinamos todo lo aprendido: sesion con streaming, suscripcion a deltas, espera de idle, y lectura de input del usuario. En consola veran un mini-chatbot funcional.

## Cierre (slide final)
Cubrimos todo lo esencial de sesiones: crear, enviar mensajes sincrona y asincronamente, suscribirnos a eventos, personalizar el system message, hacer streaming, y reanudar sesiones. En la proxima demo le damos superpoderes al modelo con herramientas personalizadas.
