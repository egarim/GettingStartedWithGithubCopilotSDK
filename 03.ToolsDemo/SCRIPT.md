# Demo 03 — Herramientas personalizadas (AIFunction)

## Intro (slide de titulo)
Las herramientas son la forma de darle al modelo capacidades que no tiene por si solo: consultar bases de datos, llamar APIs, ejecutar logica de negocio. En esta demo vamos a registrar funciones de C# como herramientas que el modelo puede invocar automaticamente. Veremos desde una herramienta simple hasta tipos complejos, manejo de errores, y filtrado de herramientas.

## step01.cs — Boilerplate y definicion de herramienta
Preparamos el cliente e importamos lo necesario, incluyendo System.ComponentModel para el atributo Description y Microsoft.Extensions.AI para AIFunctionFactory. Definimos una funcion estatica que "encripta" un string convirtiendolo a mayusculas. El atributo Description es clave: es lo que el modelo lee para decidir cuando usar la herramienta.

## step02.cs — Herramienta personalizada simple
Registramos nuestra funcion encrypt_string en la sesion usando AIFunctionFactory.Create. Le pedimos al modelo que la use, y en consola veran "HELLO WORLD". Lo importante aqui es el flujo: el modelo decide que necesita la herramienta, la invoca con los parametros correctos, recibe el resultado, y formula su respuesta. Todo automatico.

## step03.cs — Multiples herramientas
Registramos dos herramientas: get_weather y get_time. Le hacemos una pregunta que requiere ambas, y el modelo las invoca las dos para componer su respuesta. En consola veran la respuesta combinando el clima y la hora de Madrid. Esto demuestra que el modelo puede orquestar multiples herramientas en una sola interaccion.

## step04.cs — Tipos complejos de entrada y salida
Subimos la complejidad: la herramienta recibe un record con tabla, array de IDs, y un booleano de ordenamiento, y retorna un array de objetos City. Usamos source generators de System.Text.Json para la serializacion. En consola veran los parametros que recibio la herramienta y la respuesta formateada con las ciudades y sus poblaciones.

## step05.cs — Manejo de errores en herramientas
Creamos una herramienta que siempre lanza una excepcion con informacion sensible ("Melbourne"). El punto clave: el SDK captura la excepcion y NO expone el detalle al modelo. En consola veran que la respuesta dice "unknown" y que "Melbourne" no aparece. Esto es seguridad por defecto: los errores internos no se filtran al modelo.

## step06.cs — Filtros AvailableTools y ExcludedTools
Demostramos dos formas de controlar que herramientas ve el modelo. Con AvailableTools definimos una lista blanca, y con ExcludedTools una lista negra. En consola veran como cada sesion reporta herramientas diferentes. Esto es util para restringir capacidades segun el contexto o el rol del usuario.

## step07.cs — Chat interactivo con herramientas
Combinamos todo en un chat interactivo con streaming y dos herramientas registradas. El usuario puede preguntar el clima o la hora de cualquier ciudad, y el modelo invoca las herramientas necesarias en tiempo real. En consola veran la respuesta aparecer token por token mientras las herramientas se ejecutan en segundo plano.

## Cierre (slide final)
Vimos como convertir funciones de C# en herramientas del modelo usando AIFunctionFactory, desde funciones simples hasta tipos complejos con serializacion personalizada. Tambien aprendimos que el SDK protege contra filtracion de errores y que podemos filtrar herramientas por sesion. En la proxima demo vamos a interceptar las llamadas a herramientas con hooks.
