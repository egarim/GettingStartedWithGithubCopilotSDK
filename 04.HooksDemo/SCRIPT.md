# Demo 04 — Hooks Pre/Post uso de herramientas

## Intro (slide de titulo)
Los hooks nos permiten interceptar la ejecucion de herramientas antes y despues de que ocurran. Esto es fundamental para auditoria, control de acceso, y logging. En esta demo vamos a usar una herramienta de consulta de precios y vamos a ver como los hooks PreToolUse y PostToolUse nos dan control total sobre cada invocacion.

## step01.cs — Boilerplate y herramienta lookup_price
Preparamos el cliente y definimos la herramienta lookup_price, que busca precios en un diccionario simulando un catalogo de productos. Esta herramienta sera el centro de toda la demo. Notar como usamos el atributo Description para que el modelo sepa que hace y que parametro espera.

## step02.cs — Hook PreToolUse: permitir ejecucion
Configuramos un hook OnPreToolUse que se ejecuta antes de cada llamada a herramienta. En este caso simplemente permite la ejecucion retornando PermissionDecision "allow". En consola veran el log "[PreToolUse] Tool: lookup_price -> ALLOW" seguido del precio correcto de Widget Pro ($29.99). El hook ve la herramienta y la deja pasar.

## step03.cs — Hook PostToolUse: inspeccionar resultado
Ahora configuramos un hook OnPostToolUse que se ejecuta despues de que la herramienta retorna. Nos da acceso al nombre de la herramienta y al resultado que produjo. En consola veran el resultado crudo de la herramienta antes de que el modelo lo procese. Esto es perfecto para logging y auditoria de lo que realmente retornan las herramientas.

## step04.cs — Ambos hooks Pre y Post juntos
Combinamos ambos hooks en la misma sesion para ver el ciclo completo: el Pre se dispara antes mostrando la herramienta que se va a ejecutar, y el Post se dispara despues mostrando el resultado. En consola veran "[PRE] -> lookup_price" y luego "[POST] <- lookup_price: Product: Super Deluxe Widget, Price: $199.00". Es el flujo completo de observabilidad.

## step05.cs — Denegar ejecucion via PreToolUse
Aqui viene lo poderoso: el hook PreToolUse puede bloquear la ejecucion retornando "deny". La herramienta nunca se ejecuta y el modelo recibe una notificacion de que fue denegada. En consola veran que el modelo explica que no pudo acceder a la herramienta. Esto permite implementar politicas de seguridad granulares por herramienta, usuario, o contexto.

## step06.cs — Chat interactivo con hooks (aprobar/denegar en vivo)
El paso final: un chat con streaming donde cada vez que el modelo quiere usar una herramienta, el hook le pregunta al usuario si permite la ejecucion. El usuario escribe "s" o "n" en tiempo real. En consola veran el flujo completo: el usuario hace una pregunta, el modelo intenta usar lookup_price, aparece la pregunta de confirmacion, y la respuesta depende de la decision del usuario.

## Cierre (slide final)
Los hooks nos dan control total sobre la ejecucion de herramientas: podemos auditar, permitir, o denegar cada invocacion. Vimos PreToolUse para interceptar antes de la ejecucion y PostToolUse para inspeccionar resultados. En la proxima demo vamos a ver el sistema de permisos, que es similar pero aplica a las herramientas internas del SDK como editar archivos o ejecutar comandos.
