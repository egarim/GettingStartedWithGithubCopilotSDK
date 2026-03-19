# Demo 05 — Manejo de solicitudes de permisos

## Intro (slide de titulo)
Cuando el modelo necesita realizar operaciones potencialmente peligrosas como escribir archivos o ejecutar comandos, el SDK genera solicitudes de permisos. En esta demo vamos a aprender a manejar estas solicitudes: aprobar, denegar, verificar asincronamente, y manejar errores. Es el mecanismo de seguridad que permite construir aplicaciones de IA seguras y controladas.

## step01.cs — Boilerplate y directorio de trabajo
Preparamos el cliente y creamos un directorio temporal donde el modelo va a intentar escribir archivos. Este directorio es nuestro sandbox para probar las solicitudes de permisos sin afectar el sistema real. Al final de cada paso lo limpiamos.

## step02.cs — Aprobar permiso: operaciones de escritura
Configuramos OnPermissionRequest para aprobar automaticamente todas las solicitudes. Le pedimos al modelo que edite un archivo reemplazando "original" por "modified". En consola veran que el permiso fue aprobado y que el contenido del archivo efectivamente cambio. El modelo tuvo acceso a escribir porque nosotros lo autorizamos.

## step03.cs — Denegar permiso: bloquear modificaciones
Ahora hacemos lo contrario: denegamos todos los permisos con "denied-interactively-by-user". Le pedimos al modelo que modifique un archivo protegido, y en consola veran que el contenido queda intacto. El archivo sigue diciendo "protected content". Este es el mecanismo para proteger recursos criticos.

## step04.cs — Comportamiento por defecto (sin handler)
Que pasa si no configuramos OnPermissionRequest? Para operaciones que no requieren permisos especiales, como una pregunta simple de matematicas, todo funciona normal. En consola veran la respuesta "4". Los permisos solo se solicitan cuando el modelo necesita ejecutar acciones con efectos secundarios.

## step05.cs — Handler asincrono de permisos
El handler de permisos puede ser asincrono, lo que permite consultar una base de datos, llamar una API externa, o mostrar un dialogo al usuario antes de decidir. Aqui simulamos una verificacion con un delay de 500ms. En consola veran el mensaje "Verificando..." seguido de "Aprobado tras espera". En produccion esto podria ser una llamada a un servicio de autorizacion.

## step06.cs — ToolCallId en solicitudes de permisos
Cada solicitud de permiso incluye un ToolCallId unico que permite correlacionar la solicitud con la herramienta especifica que la genero. En consola veran el ID impreso. Esto es esencial para auditoria: puedes saber exactamente que herramienta pidio que permiso y cuando.

## step07.cs — Error en handler: degradacion elegante
Que pasa si nuestro handler de permisos lanza una excepcion? El SDK lo maneja de forma segura: en vez de crashear la aplicacion, deniega automaticamente el permiso. En consola veran que la excepcion se dispara pero la aplicacion sigue funcionando, y el modelo reporta que no pudo ejecutar la accion. Fail-safe por defecto.

## step08.cs — Handler de permisos al reanudar sesion
Cuando reanudamos una sesion con ResumeSessionAsync, podemos adjuntar un nuevo handler de permisos via ResumeSessionConfig. En consola veran que al reanudar la sesion y solicitar una operacion que requiere permisos, el nuevo handler se dispara correctamente. Los permisos no se heredan de la sesion original.

## step09.cs — Chat interactivo con permisos (aprobar/denegar en vivo)
El cierre practico: un chat en streaming donde cada vez que el modelo necesita un permiso, se le pregunta al usuario en consola si lo aprueba. Veran el Kind del permiso, el ToolCallId, y la pregunta "Aprobar? (s/n)". Esto simula exactamente la experiencia que tendria un usuario final en una aplicacion de produccion.

## Cierre (slide final)
El sistema de permisos es la linea de defensa entre el modelo y las acciones con efectos secundarios. Vimos como aprobar, denegar, verificar asincronamente, manejar errores, y persistir permisos entre sesiones. En la proxima demo veremos el mecanismo de AskUser, donde el modelo puede hacerle preguntas directas al usuario.
