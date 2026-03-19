# Demo 09 — Servidores MCP y agentes personalizados

## Intro (slide de titulo)
Esta es una de las demos mas completas del curso. Vamos a explorar dos conceptos poderosos: los servidores MCP (Model Context Protocol) que extienden las capacidades del modelo con herramientas externas, y los agentes personalizados que permiten definir roles especializados. Veremos como combinarlos para crear arquitecturas de agentes sofisticadas.

## step01.cs — Estructura base
Iniciamos el cliente Copilot con la configuracion estandar. Este paso establece la conexion y confirma que todo funciona antes de agregar la complejidad de MCP y agentes.

## step02.cs — Configuracion de un servidor MCP
Aqui configuramos nuestro primer servidor MCP usando `McpServers` en la sesion. Definimos un servidor local con un comando, argumentos y acceso a todas sus herramientas con el wildcard `["*"]`. En la consola van a ver que la sesion funciona normalmente — el servidor MCP queda disponible como una extension que el modelo puede invocar cuando lo necesite.

## step03.cs — Multiples servidores MCP
Ahora escalamos a dos servidores MCP en la misma sesion: uno para filesystem y otro para base de datos. Fijense que cada servidor tiene su propia configuracion independiente. Esto demuestra que una sesion puede orquestar multiples fuentes de datos y herramientas simultaneamente. En la consola veremos el ID de la sesion confirmando que ambos servidores estan activos.

## step04.cs — Configuracion de un agente personalizado
Pasamos al segundo concepto clave: los agentes. Creamos un `CustomAgentConfig` con nombre, descripcion y un prompt que define su personalidad. La propiedad `Infer = true` permite que el modelo decida automaticamente cuando delegar a este agente. Van a ver que la sesion responde normalmente, pero ahora tiene un especialista en analisis de negocio disponible.

## step05.cs — Agente con herramientas restringidas
Este paso muestra como limitar las herramientas de un agente con la propiedad `Tools`. Nuestro agente DevOps solo puede usar "bash" y "edit" — no tiene acceso a todas las herramientas. Esto es fundamental para seguridad en produccion: cada agente solo puede hacer lo que explicitamente le permitimos.

## step06.cs — Agente con servidores MCP propios
Aqui combinamos ambos conceptos: un agente que tiene sus propios servidores MCP aislados. Los servidores MCP definidos dentro del agente son exclusivos de ese agente — otros agentes o la sesion principal no los ven. En la consola veremos la confirmacion de que el agente tiene su MCP propio y aislado.

## step07.cs — Multiples agentes en una sesion
Configuramos dos agentes especializados: uno para frontend y otro para backend. Fijense en la diferencia: el agente frontend tiene `Infer` habilitado (el modelo decide cuando usarlo), mientras que el backend tiene `Infer = false` (solo se invoca explicitamente). Esto permite controlar exactamente como se orquestan los agentes.

## step08.cs — MCP + Agentes combinados
Este es el paso de integracion: una sesion con un servidor MCP compartido y un agente coordinador que puede acceder a esos servidores. El agente actua como orquestador que decide cuando y como usar las herramientas MCP disponibles. Van a ver la respuesta en consola confirmando que todo funciona en conjunto.

## step09.cs — MCP y agentes al reanudar sesion
Un patron avanzado: creamos una sesion, enviamos un mensaje, y luego la reanudamos con `ResumeSessionAsync` agregando servidores MCP y agentes que no existian en la sesion original. Esto es potente para escenarios donde las capacidades se van activando dinamicamente segun el contexto de la conversacion.

## step10.cs — Modo interactivo con agente
Cerramos con un chat interactivo que usa un agente personalizado con streaming. Van a poder escribir mensajes y ver como el agente responde en tiempo real. Esto representa el patron completo de una aplicacion de chat con agentes en produccion.

## Cierre (slide final)
Vimos como los servidores MCP extienden las capacidades del modelo con herramientas externas, y como los agentes personalizados permiten crear roles especializados con permisos controlados. La combinacion de ambos permite arquitecturas de agentes sofisticadas y seguras. En la siguiente demo veremos como usar modelos custom con BYOK a traves de OpenRouter.
