# Demo 11 — Modelos custom con BYOK (Bring Your Own Key)

## Intro (slide de titulo)
En esta demo vamos a explorar una de las funcionalidades mas flexibles del Copilot SDK: la capacidad de usar modelos custom mediante BYOK (Bring Your Own Key). Veremos como listar modelos disponibles, seleccionar uno especifico, comparar respuestas entre modelos, y combinar modelos custom con herramientas y streaming.

## step01.cs — Autenticacion y verificacion
Comenzamos con un patron importante: la doble estrategia de autenticacion. Si existe un `GITHUB_TOKEN` en las variables de entorno lo usamos como PAT; si no, caemos al login de VS Code. En la consola veremos el estado de autenticacion y la fuente que se uso. Esto es clave para entornos CI/CD donde no hay VS Code disponible.

## step02.cs — Listar modelos disponibles
Usamos `ListModelsAsync` para obtener todos los modelos disponibles, tanto los built-in de Copilot como los modelos BYOK que hayas configurado. En la consola veremos la lista completa con ID y nombre de cada modelo. Esta lista es la base para seleccionar modelos en los siguientes pasos.

## step03.cs — Chat con el modelo por defecto
Aqui creamos una sesion sin especificar modelo — el SDK elige el modelo por defecto. Le preguntamos al modelo que se identifique para confirmar cual se esta usando. En la consola veremos la respuesta que nos dice exactamente que modelo esta detras.

## step04.cs — Seleccionar un modelo especifico
Ahora usamos la propiedad `Model` en `SessionConfig` para elegir explicitamente "gpt-4o". Esto funciona igual con cualquier modelo de la lista que obtuvimos en el paso 2, incluyendo modelos BYOK. Van a ver en la consola tanto el modelo seleccionado como su respuesta auto-identificandose.

## step05.cs — Comparar multiples modelos
Este es uno de los pasos mas interesantes: tomamos los primeros 4 modelos disponibles y les enviamos exactamente el mismo prompt a cada uno. En la consola veremos las respuestas lado a lado, permitiendo comparar calidad, velocidad y estilo de cada modelo. Fijense que usamos try/catch porque algunos modelos podrian no estar disponibles.

## step06.cs — Modelo custom con herramientas (tools)
Combinamos un modelo especifico con function calling. Definimos dos funciones locales — clima y hora — y las registramos como herramientas usando `AIFunctionFactory.Create`. Le pedimos al modelo que use ambas herramientas en una sola consulta. En la consola veremos como el modelo orquesta las llamadas a las funciones y compone una respuesta integrada.

## step07.cs — Streaming con modelo custom
Habilitamos streaming con un modelo BYOK. La respuesta llega token por token en tiempo real gracias al evento `AssistantMessageDeltaEvent`. En la consola van a ver el texto apareciendo progresivamente, y al final el conteo total de caracteres. Esto confirma que streaming funciona igual de bien con modelos custom.

## step08.cs — Chat interactivo con modelo BYOK
Cerramos con el loop interactivo completo: streaming, modelo custom y manejo de errores. Van a poder chatear libremente con el modelo seleccionado. Fijense que agregamos manejo del evento `SessionErrorEvent` para capturar errores sin romper el loop. Esto representa la implementacion completa de un chat con modelo BYOK en produccion.

## Cierre (slide final)
Vimos que el Copilot SDK no te limita a un solo modelo — con BYOK puedes usar cualquier modelo disponible en tu cuenta, compararlos, combinarlos con herramientas y streaming, y cambiar entre ellos con una sola linea de configuracion. Esto abre la puerta a arquitecturas multi-modelo donde cada tarea usa el modelo mas adecuado.
