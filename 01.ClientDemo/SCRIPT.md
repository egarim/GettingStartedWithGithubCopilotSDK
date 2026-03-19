# Demo 01 — Ciclo de vida y conexion del cliente

## Intro (slide de titulo)
En esta primera demo vamos a explorar el ciclo de vida completo de un CopilotClient. Veremos como crear el cliente, conectarlo al servidor, verificar que esta vivo, consultar su estado y autenticacion, listar los modelos disponibles, y finalmente detenerlo de forma ordenada. Es la base sobre la que se construyen todas las demos siguientes.

## step01.cs — Imports y logger
Empezamos con lo minimo: importamos el SDK de Copilot y configuramos un logger de consola con nivel Warning para no llenar la pantalla de ruido. Esto es boilerplate que vamos a reutilizar en cada demo. En consola solo veran el titulo del demo.

## step02.cs — Factory del cliente y helpers de impresion
Agregamos una funcion factory que crea un CopilotClient con UseLoggedInUser en true, lo que significa que usara las credenciales de GitHub del usuario actual. Tambien definimos dos helpers para imprimir los pasos de forma legible. Todavia no conectamos nada, solo preparamos la infraestructura.

## step03.cs — Crear el cliente
Aqui instanciamos el cliente por primera vez y consultamos su estado inicial. Van a ver en consola que el estado es "Created" o similar, lo que indica que el objeto existe pero aun no se ha conectado al servidor. Cerramos con DisposeAsync, que es el patron correcto para liberar recursos.

## step04.cs — Iniciar el cliente con StartAsync
Ahora si llamamos a StartAsync, que es una operacion asincrona que conecta al servidor de Copilot. Despues de iniciar, el estado cambia a "Running". Este es el momento en que el cliente esta listo para recibir comandos. Fijense en como el estado transiciona de Created a Running.

## step05.cs — Ping al servidor
Usamos PingAsync para verificar que la conexion esta viva. Le enviamos un mensaje y recibimos de vuelta un pong con el mismo mensaje y un timestamp. En consola veran el mensaje enviado, la respuesta eco, y la marca de tiempo del servidor. Es un health check basico.

## step06.cs — Estado del servidor
Con GetStatusAsync obtenemos informacion del servidor: la version del SDK y la version del protocolo de comunicacion. Esto es util para diagnostico y para verificar compatibilidad entre cliente y servidor. En consola aparecen ambos numeros de version.

## step07.cs — Estado de autenticacion
Llamamos a GetAuthStatusAsync para saber si estamos autenticados correctamente. En consola veran si IsAuthenticated es true, el tipo de autenticacion que se uso, y un mensaje de estado. Si la autenticacion falla aqui, nada de lo que sigue va a funcionar.

## step08.cs — Listar modelos disponibles
Con ListModelsAsync consultamos todos los modelos de IA a los que tenemos acceso. En consola aparece la cantidad de modelos y una lista con su ID y nombre. Dependiendo de la suscripcion del usuario, veran modelos como GPT-4o, Claude, etc. Esto nos permite elegir modelo al crear sesiones.

## step09.cs — Parada ordenada con StopAsync
Finalmente, llamamos a StopAsync para desconectar limpiamente del servidor antes de destruir el cliente con DisposeAsync. El estado vuelve a "Stopped". Es importante seguir este patron de parada ordenada para liberar recursos del lado del servidor.

## Cierre (slide final)
Recapitulando: vimos el ciclo completo Created, Running, Stopped de un CopilotClient. Aprendimos a verificar conectividad con Ping, consultar el estado del servidor y autenticacion, y listar modelos disponibles. En la siguiente demo vamos a crear sesiones y tener conversaciones reales con el modelo.
