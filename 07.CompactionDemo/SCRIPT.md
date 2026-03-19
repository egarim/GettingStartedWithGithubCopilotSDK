# Demo 07 — Sesiones infinitas y compactacion de contexto

## Intro (slide de titulo)
En esta demo vamos a explorar una de las funcionalidades mas importantes para aplicaciones de chat en produccion: las sesiones infinitas. Veremos como el SDK gestiona automaticamente la compactacion del contexto para que una conversacion pueda durar indefinidamente sin agotar la ventana de tokens.

## step01.cs — Estructura base
Comenzamos con la estructura basica del cliente Copilot. Aqui simplemente inicializamos el cliente con el logger configurado para mostrar solo advertencias, e imprimimos el titulo del demo. Esto nos da la base sobre la cual construiremos la configuracion de compactacion.

## step02.cs — Compactacion activada con umbrales bajos
Ahora configuramos una sesion con `InfiniteSessions` habilitado. Fijense que usamos umbrales extremadamente bajos — 0.5% y 1% — para que la compactacion se active rapido en el demo. Nos suscribimos a los eventos `SessionCompactionStartEvent` y `SessionCompactionCompleteEvent` para ver en la consola cuando el SDK decide compactar y cuantos tokens removio. Van a ver como la IA responde normalmente mientras la compactacion trabaja en segundo plano.

## step03.cs — Compactacion desactivada (linea base)
Aqui hacemos lo contrario: desactivamos la compactacion poniendo `Enabled = false`. El proposito es demostrar que cuando la compactacion esta apagada, no se generan eventos de compactacion. En la consola van a ver que el contador de eventos queda en cero. Esto sirve como punto de comparacion para confirmar que los eventos del paso anterior realmente vinieron de la compactacion.

## step04.cs — Sesion infinita interactiva con streaming
Este es el paso mas completo: combinamos sesion infinita, streaming en tiempo real y un loop interactivo. El usuario puede chatear indefinidamente y en el prompt se muestra cuantas compactaciones han ocurrido. Cada vez que el contexto crece demasiado, veran el mensaje de compactacion aparecer en la consola. Esto simula exactamente como funcionaria un chat en produccion — la experiencia del usuario no se interrumpe mientras el SDK gestiona la memoria automaticamente.

## Cierre (slide final)
Vimos como las sesiones infinitas permiten conversaciones sin limite de longitud gracias a la compactacion automatica del contexto. Los umbrales son configurables para adaptar el comportamiento a las necesidades de cada aplicacion. En la siguiente demo veremos como extender las capacidades del modelo usando Skills.
