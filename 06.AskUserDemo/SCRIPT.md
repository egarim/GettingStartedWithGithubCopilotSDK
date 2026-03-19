# Demo 06 — Solicitudes de entrada del usuario (AskUser)

## Intro (slide de titulo)
A veces el modelo necesita informacion que solo el usuario puede darle: una preferencia, una confirmacion, una eleccion entre opciones. El mecanismo AskUser permite que el modelo haga preguntas al usuario a traves de nuestro codigo. En esta demo veremos como manejar preguntas con opciones predefinidas, respuestas de texto libre, y un chat interactivo donde el modelo puede preguntar en tiempo real.

## step01.cs — Boilerplate del cliente
Preparamos el cliente con la estructura habitual. Este paso solo muestra el titulo del demo en consola. Lo importante es que la siguiente funcionalidad que vamos a agregar es OnUserInputRequest, el callback que se dispara cuando el modelo necesita input del usuario.

## step02.cs — Entrada con opciones (auto-responder primera opcion)
Configuramos OnUserInputRequest para manejar las preguntas del modelo. Cuando la pregunta incluye opciones predefinidas (Choices), auto-seleccionamos la primera. En consola veran la pregunta del modelo, la lista de opciones, y cual se selecciono automaticamente. Notar que WasFreeform es false porque elegimos de una lista, no escribimos texto libre.

## step03.cs — Verificar opciones en UserInputRequest
Profundizamos en la estructura de UserInputRequest: recolectamos todas las solicitudes y verificamos cuantas incluyeron opciones. En consola veran las opciones numeradas (Red, Blue) y la confirmacion de que hubo exactamente una solicitud con opciones. Esto es util para validar que el modelo esta usando ask_user correctamente.

## step04.cs — Entrada libre del usuario (WasFreeform = true)
Ahora respondemos con texto libre en vez de elegir de una lista. Le pedimos al modelo que pregunte nuestro color favorito, y respondemos "emerald green". En consola veran que WasFreeform es true y que la respuesta del modelo incorpora nuestra respuesta textual. La diferencia entre freeform y seleccion de opciones es importante para la UX de la aplicacion.

## step05.cs — Chat interactivo con AskUser en vivo
El paso final integra todo: un chat en streaming donde el modelo puede hacernos preguntas en tiempo real. Si la pregunta tiene opciones, las mostramos numeradas y el usuario puede elegir por numero o escribir texto libre. En consola veran el flujo completo: el usuario pide algo, el modelo necesita mas informacion, aparece la pregunta con opciones, el usuario responde, y el modelo continua con esa informacion.

## Cierre (slide final)
AskUser cierra el ciclo de comunicacion bidireccional entre el modelo y el usuario. Combinado con herramientas, hooks, y permisos, tenemos todas las piezas para construir aplicaciones de IA interactivas, seguras, y completamente personalizables con el Copilot SDK. Esto concluye la serie de demos fundamentales.
