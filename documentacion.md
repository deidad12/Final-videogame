# Documentación del Videojuego: Pastel Dream World

## 1. Ficha Técnica
*   **Nombre del Videojuego:** Pastel Dream World
*   **Tipo de Juego (Género):** Plataformas 2D Estilo Retro / Mario
*   **Clasificación de Edad:** PEGI 3 / Clasificación A (Para todo público)
*   **Tecnología:** Unity 2022+ / C#
*   **Estilo Visual:** Colores rosa pastel, lila, celeste y estética suave (Kawaii/Chibi).

---

## 2. Historia del Videojuego
En el pacífico reino de *Lollipop Valley*, todo era armonía, risas y colores pasteles. Sus habitantes vivían felices rodeados de montañas de algodón de azúcar y ríos de fresa. 

Sin embargo, una misteriosa fuerza de tonos oscuros e industriales ha empezado a robar la dulzura y el color del reino. El malvado emperador *Dr. Cocoa* y sus secuaces se han apoderado de las tres fuentes principales de caramelo.

Nuestra heroína, armada únicamente con su agilidad para saltar sobre las cabezas de los invasores y su valentía, debe viajar a través de tres mundos mágicos, esquivar trampas espinosas, evitar los proyectiles de las torretas de caramelo amargo, esquivar el peso de los aplastadores y derrotar al mismísimo Dr. Cocoa en su propia fortaleza. ¡Solo ella puede restaurar los tonos rosa y la paz en Lollipop Valley!

---

## 3. Estructura de Niveles y Diseño de Escenas

### Escena de Menú Inicial (`MainMenu`)
Un menú con interfaz de tonos rosa translúcido y botones color lavanda. Cuenta con música de fondo alegre e introduce las opciones para empezar a jugar (`Jugar`), ver controles/opciones o salir del juego.

### Nivel 1: Valle de Algodón de Azúcar (`Level1`)
*   **Objetivo:** Cruzar el primer valle, aprender las mecánicas básicas de saltar, esquivar pinchos y recolectar las primeras estrellas de caramelo.
*   **Obstáculos y Enemigos:** 
    *   **Pinchos de Caramelo Amargo (ObstacleSpikes):** Trampas estáticas en el suelo.
    *   **Patrullero de Chocolate (EnemyPatrol):** Enemigo terrestre que camina de lado a lado. Se le puede vencer saltando en su cabeza.
*   **Detalle:** El nivel sirve de tutorial. Si el jugador muere o se acaba el tiempo de 60 segundos, se muestra el letrero "¡Te quedaste sin vidas!" o "¡Se agotó el tiempo!", se reproduce la música de derrota y se reinicia el nivel desde el spawn.

### Nivel 2: Las Nubes de Fresa (`Level2`)
*   **Objetivo:** Cruzar plataformas suspendidas y llegar al portal de victoria antes de que el tiempo acabe.
*   **Obstáculos y Enemigos:**
    *   **Globo Flotante (EnemyFlyer):** Enemigo aéreo que sube y baja dificultando los saltos de precisión.
    *   **Torreta Disparadora (ObstacleShooter):** Lanza proyectiles horizontales a intervalos regulares que el jugador debe saltar.
    *   **Aplastador (ObstacleThwomp):** Un bloque pesado de piedra de chocolate que cae de repente si el jugador pasa por debajo de él.
*   **Detalle:** Si el jugador se queda sin vidas o falla en completar este nivel, se muestra el panel indicando su derrota y se le penaliza **regresándolo automáticamente al Nivel 1**.

### Nivel 3: El Castillo de Fresa y Batalla de Jefe (`Level3`)
*   **Objetivo:** Enfrentarse en un combate directo al gran Boss *Dr. Cocoa*.
*   **Música:** Cambia de forma dinámica a una música de batalla de jefe mucho más épica y rítmica.
*   **El Jefe Final (BossController):**
    *   Posee **3 Puntos de Vida (HP)**.
    *   Se mueve rápidamente de lado a lado e intenta aplastar al jugador saltando de forma aleatoria.
    *   Con cada pisotón que recibe del jugador, se enfurece (parpadea en rojo), aumentando su velocidad de movimiento y la frecuencia de sus saltos.
*   **Detalle:** Tras golpearlo 3 veces, el jefe es derrotado y se activa automáticamente la pantalla de Victoria General del juego. Si el jugador pierde aquí, también **es devuelto al Nivel 1**.

---

## 4. Guion del Videojuego e Interacciones
*   **Entrada en el juego:** El jugador pulsa "Jugar" $\rightarrow$ La cámara se inicializa y tiñe la pantalla de un rosa suave gracias al `AestheticManager`.
*   **Recolección de Objetos:** Al tocar una estrella, suena un timbre agudo y dulce, sumando 1 al HUD de "Objetos".
*   **Golpear Bloques:** Al saltar debajo de un bloque de interrogación, el bloque salta físicamente, emite un sonido sordo y hace aparecer una estrella brillante, convirtiéndose en un bloque inactivo de piedra.
*   **Muerte del personaje:** Al perder la última vida o expirar el temporizador, el juego se pausa físicamente. El panel rosa pastel aparece en el centro mostrando: **"¡Te quedaste sin vidas! Regresando al Nivel 1..."** y suena una melodía de derrota.

---

## 5. Descripción del Sistema Técnico (Scripts C#)
1.  **`GameManager.cs`:** Singleton principal de estado. Controla las vidas, el tiempo, la recolección, las transiciones ordenadas entre niveles (1 $\rightarrow$ 2 $\rightarrow$ 3 $\rightarrow$ Victoria) y los retrocesos al Nivel 1 por muerte.
2.  **`AestheticManager.cs`:** Diseñador dinámico. Colorea la cámara en rosa pastel y da estética premium, bordes y fuentes de colores lavanda/marrón a los botones y paneles en ejecución.
3.  **`PlayerMovement.cs`:** Controla la física del jugador y clasifica las colisiones. Si golpea desde arriba, es ataque Stomp; si es de lado, recibe daño; si es desde abajo contra un bloque, lo activa.
4.  **`AudioManager.cs`:** Reproductor persistente. Ajusta volumen, maneja efectos y cambia música dependiendo de si la escena cargada es el Boss o no.
5.  **`EnemyPatrol.cs` / `EnemyFlyer.cs`:** Enemigos con inteligencia artificial básica de movimiento.
6.  **`ObstacleSpikes.cs` / `ObstacleShooter.cs` / `ObstacleThwomp.cs`:** Trampas y proyectiles reactivos.
7.  **`BossController.cs`:** Máquina de estados del jefe final (HP, invulnerabilidad y escalado de dificultad).

---

## 6. Enlaces del Proyecto
*   **Repositorio GitHub:** [https://github.com/Usuario/PastelDreamWorld-2D](https://github.com/Usuario/PastelDreamWorld-2D) *(Link ilustrativo para la entrega, configurable por el alumno)*
*   **Publicación en Itch.io:** [https://usuario.itch.io/pastel-dream-world](https://usuario.itch.io/pastel-dream-world) *(Link ilustrativo para la entrega, configurable por el alumno)*
