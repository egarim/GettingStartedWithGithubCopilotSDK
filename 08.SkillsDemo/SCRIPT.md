# Demo 08 — Skills personalizados

## Intro (slide de titulo)
En esta demo vamos a aprender a crear y usar Skills con el Copilot SDK. Los Skills son instrucciones empaquetadas en archivos SKILL.md que modifican el comportamiento del modelo sin tocar codigo. Veremos como crearlos, cargarlos, desactivarlos y compararlos contra una sesion sin skills.

## step01.cs — Crear el archivo SKILL.md
Lo primero es entender la estructura de un Skill: es simplemente un archivo Markdown con un frontmatter YAML que define nombre y descripcion, seguido de las instrucciones. En este paso creamos un skill llamado "demo-skill" que obliga al modelo a incluir un marcador unico en cada respuesta. Van a ver en la consola la ruta donde se creo el archivo — no hay magia, es un simple archivo de texto.

## step02.cs — Cargar y aplicar el skill
Ahora conectamos el skill con una sesion usando la propiedad `SkillDirectories`. El SDK escanea automaticamente todos los subdirectorios buscando archivos SKILL.md. Le pedimos al modelo que responda y verificamos si la respuesta contiene nuestro marcador. En la consola van a ver que el marcador aparece — confirmando que el skill esta activo e inyectando instrucciones al modelo.

## step03.cs — Desactivar un skill por nombre
Aqui viene algo clave para produccion: poder desactivar skills selectivamente usando `DisabledSkills`. Cargamos el mismo directorio de skills pero desactivamos "demo-skill" por nombre. Van a ver que ahora la respuesta ya no contiene el marcador, demostrando que el SDK respeta la lista de exclusion sin necesidad de borrar archivos.

## step04.cs — Comparacion sin skill (linea base)
Este paso es nuestra linea base: creamos una sesion sin ninguna `SkillDirectories`. La respuesta no tiene marcador, como era de esperar. Esto nos permite confirmar que el comportamiento del skill en los pasos anteriores era realmente por el skill y no algo inherente del modelo.

## step05.cs — Modo interactivo con skill personalizado
Cerramos con un ejercicio practico: creamos un segundo skill que obliga al modelo a terminar todas las respuestas con signo de exclamacion, y abrimos un chat interactivo con streaming. En la consola van a poder chatear libremente y verificar que cada respuesta respeta ambos skills cargados del directorio. Esto muestra el poder de componer multiples skills en una sola sesion.

## Cierre (slide final)
Los Skills son una forma elegante de personalizar el comportamiento del modelo sin hardcodear instrucciones en el codigo. Se pueden versionar, compartir y activar o desactivar segun el contexto. En la siguiente demo veremos como integrar servidores MCP y agentes personalizados.
