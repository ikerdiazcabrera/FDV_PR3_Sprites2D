# Práctica 3 – Introducción a Unity 2D: Sprites

**Asignatura:** Fundamentos de Desarrollo de Videojuegos
**Motor:** Unity 6000.5.11f1 (plantilla Universal 2D)
**Assets:** sprites del proyecto final *El Ojo del Rey* (guerrero protagonista y el Ojo), importados en crudo, sin sus `.meta`, para configurarlos desde cero.

## Índice
1. [Atlas de sprites](#ejercicio-1--atlas-de-sprites)
2. [Animaciones a partir de sprites](#ejercicio-2--animaciones-a-partir-de-sprites)
3. [Movimiento horizontal y orientación (Ej. 3–6)](#ejercicios-3-a-6--movimiento-horizontal-y-orientación)
4. [Animator y cambio de animación por distancia](#ejercicio-7--animator-y-cambio-de-animación-por-distancia)
5. [Pixels Per Unit (PPU)](#ejercicio-8--pixels-per-unit-ppu)
6. [Sprites con PPU distinto](#ejercicio-9--sprites-con-ppu-distinto)
7. [Cámara ortográfica y relación de aspecto](#ejercicio-10--cámara-ortográfica-y-relación-de-aspecto)
8. [Scripts](#scripts)

---

## Ejercicio 1 – Atlas de sprites

Un **atlas de sprites** es una única imagen que contiene muchos sprites: los frames de las animaciones, objetos, efectos… Cargar una sola textura es más eficiente que cargar cada imagen por separado, porque se reducen las texturas en memoria y los cambios de textura al dibujar (*draw calls*).

Pasos realizados con el spritesheet del guerrero:
- Al importarlo, Unity lo trata como **Sprite Mode = Single** con **Pixels Per Unit = 100**.
- Lo he cambiado a **Sprite Mode = Multiple**, con **Filter Mode = Point** y **Compression = None**, que es la configuración propia del pixel art.
- En el **Sprite Editor** lo he cortado con *Slice → Grid By Cell Size* de **69 × 44 px**, el tamaño de cada frame.
- He arrastrado uno de los sprites a la escena para crear el personaje.

![Ejercicio 1 – Configuración del atlas en el Inspector](Assets/Media/Ej1.png)

![Ejercicio 1 – Sprite del personaje en la escena](Assets/Media/Ej1.2.png)

---

## Ejercicio 2 – Animaciones a partir de sprites

Al seleccionar un conjunto de frames del atlas y arrastrarlos sobre el objeto en la Hierarchy, Unity crea automáticamente un **Animation Clip** (`.anim`). La primera vez, además, crea un **Animator Controller** y añade el componente **Animator** al objeto. Cada nuevo conjunto que se arrastra se añade como un estado más del mismo controller.

Clips creados:
| Clip | Frames | Uso |
|---|---|---|
| `PR3_Idle` | Idle | Personaje parado (estado por defecto) |
| `PR3_Run` | Run | Personaje caminando |
| `PR3_Cansado` | Crouch | Animación que se activa tras recorrer cierta distancia (Ej. 7) |

![Ejercicio 2 – Clips y Animator Controller generados](Assets/Media/Ej2.png)

![Ejercicio 2 – Animaciones en ejecución](Assets/Media/PR3_Ej2.gif)

---

## Ejercicios 3 a 6 – Movimiento horizontal y orientación

El script `PR3_MovimientoHorizontal` mueve al personaje solo en el eje X modificando su `transform.position` mediante `transform.Translate`:

```csharp
Vector2 movement = new Vector2(horizontalInput, 0f);
transform.Translate(movement * speed * Time.deltaTime);
```

- **Entrada por teclado (Ej. 4):** el personaje solo avanza mientras se pulsan las flechas izquierda/derecha (o A/D). Se usa el **Input System nuevo**, pero el eje se calcula imitando a `Input.GetAxis("Horizontal")`: sube y baja de forma gradual, y vuelve a 0 de golpe al cambiar de sentido. Desactivando *Suavizar Eje* se comporta como `GetAxisRaw`.
- **`Time.deltaTime`:** hace que la velocidad sea en unidades por segundo e independiente de los FPS.
- **Orientación (Ej. 6):** la referencia al `SpriteRenderer` se obtiene **una sola vez** en `Awake`. En cada frame se comprueba el signo de `horizontalInput`:
  - si es positivo, el sprite mira a la derecha (`flipX = false`);
  - si es negativo, mira a la izquierda (`flipX = true`);
  - si es 0, conserva la última orientación.

![Ejercicios 3–6 – Movimiento y flip del sprite](Assets/Media/PR3_Ej3-6.gif)

---

## Ejercicio 7 – Animator y cambio de animación por distancia

### Máquina de estados
En la pestaña **Animator** se ha definido la máquina de estados con dos parámetros Bool, `isWalking` y `cansado`. Desde *Entry* se pasa a **Idle**, el estado por defecto. Todas las transiciones tienen *Has Exit Time* desactivado y *Transition Duration = 0*, para que el cambio sea inmediato, como corresponde al pixel art.

| Transición | Condiciones |
|---|---|
| Idle → Run | `isWalking = true`, `cansado = false` |
| Run → Idle | `isWalking = false` |
| Run → Cansado | `cansado = true` |
| Idle → Cansado | `isWalking = true`, `cansado = true` |
| Cansado → Idle | `isWalking = false` |

`PR3_MovimientoHorizontal` actualiza `isWalking` con `animator.SetBool("isWalking", …)`.

### Script de distancia
`PR3_CambioAnimacionDistancia`:
- obtiene la referencia al **Animator** una sola vez;
- tiene una variable **`distanciaActivacion`**, configurable desde el Inspector (10 u);
- en cada frame calcula cuánto se ha movido el personaje respecto a la posición del frame anterior y lo **acumula**:
  ```csharp
  float distanceThisFrame = Vector3.Distance(posicionAnterior, transform.position);
  distanciaRecorrida += distanceThisFrame;
  posicionAnterior = transform.position;
  ```
- cuando la distancia acumulada supera la de activación, ejecuta `animator.SetBool("cansado", true)` y el personaje pasa a la animación *Cansado*.

**Distancia acumulada frente a distancia desde el inicio:** el script también muestra `Vector3.Distance(firstPosition, transform.position)`. Si el personaje va y vuelve, esa distancia no crece, porque es el desplazamiento neto. La acumulada sí crece. Por eso, para medir "cuánto ha caminado", hay que sumar la distancia de cada frame.

![Ejercicio 7 – Cambio de animación tras recorrer la distancia](Assets/Media/PR3_Ej7.gif)

---

## Ejercicio 8 – Pixels Per Unit (PPU)

El **PPU** indica cuántos píxeles del sprite ocupan **1 unidad** del mundo de Unity:

> **tamaño en unidades = tamaño en píxeles / PPU**

- **Tamaño de un sprite del personaje:** 69 × 44 px.
- **PPU por defecto:** 100.

| PPU | Tamaño del personaje (unidades) |
|---|---|
| 100 | 0,69 × 0,44 |
| 32 | 2,16 × 1,375 |
| 16 | 4,31 × 2,75 |

Los valores se han comprobado con un **BoxCollider2D**, cuyo *Size* se ajusta al sprite, con la cuadrícula de la vista Scene y con el script `PR3_InfoSprite`.

**Al cambiar el PPU, ¿ha cambiado la imagen?**
No. La textura tiene exactamente los mismos píxeles. Lo que cambia es la **escala con la que Unity la representa en el mundo**: con un PPU más bajo caben menos píxeles en cada unidad, así que el sprite ocupa más unidades y se ve más grande. Con un PPU más alto ocurre lo contrario.

**PPU elegido para que mida entre 1 y 2 unidades de alto: 32.**
Cualquier valor entre 22 y 44 deja el frame entre 1 y 2 unidades. Con 32 el frame mide 1,375 u y el cuerpo del personaje (unos 32 px de alto) mide justo **1 unidad**. Además, 32 es potencia de 2, como el tamaño habitual de los tiles, así que el personaje encaja con la cuadrícula del nivel.

*Nota:* en el proyecto final se usa **PPU 16** porque el tileset es de 16 × 16 px. Así un tile mide 1 unidad y el personaje mide 2 tiles de alto. Lo importante es que todos los assets compartan el mismo PPU.

![Ejercicio 8 – Cambio de PPU y tamaño del personaje](Assets/Media/PR3_Ej8.gif)

---

## Ejercicio 9 – Sprites con PPU distinto

Se ha añadido un segundo sprite, el Ojo, con **PPU 100**, junto al personaje, que tiene **PPU 16**. Para que tengan un tamaño parecido en pantalla ha habido que aumentar bastante la *Scale* del Ojo. Para que no se vieran borrosos se ha puesto **Filter Mode = Point (no filter)** y **Compression = None**.

**¿Los píxeles de los dos sprites tienen el mismo tamaño?**
No. Al hacer zoom se ve que los píxeles de un sprite son más grandes que los del otro. El tamaño de un píxel en el mundo es `escala / PPU`, que `PR3_InfoSprite` muestra por consola. Como los dos sprites tienen PPU y escala distintos, sus píxeles miden distinto. Esto rompe la coherencia visual del pixel art: unos objetos parecen más definidos que otros.

**¿Qué regla deberían cumplir todos los sprites del juego?**
Todos deben usar el **mismo PPU** y mantener la **escala a 1**, o como mucho escalas enteras. Así un píxel de cualquier sprite ocupa siempre el mismo espacio en pantalla. Además, todos deben usar **Filter Mode = Point** y **Compression = None**.

![Ejercicio 9 – Comparación de sprites con distinto PPU](Assets/Media/PR3_Ej9.gif)

---

## Ejercicio 10 – Cámara ortográfica y relación de aspecto

La Main Camera usa **Projection = Orthographic** con **Size = 5**. El *Size* es la **mitad de la altura visible** en unidades.

**Cálculo previo:** altura visible = 2 × Size = 2 × 5 = **10 unidades**.
**Comprobación:** se han colocado dos objetos en y = +5 e y = −5. Su centro queda justo en los bordes superior e inferior de la vista Game, así que solo se ve la mitad de cada uno. También se han puesto marcadores en los bordes laterales con `PR3_InfoCamara`.

![Ejercicio 10 – Marcadores en los bordes de la vista de cámara](Assets/Media/Ej10.png)

**Cambio de Size:**
- **Size = 2:** se ven 4 u de alto, así que el personaje se ve **más grande** (2,5 veces respecto a Size 5).
- **Size = 10:** se ven 20 u de alto, así que el personaje se ve **a la mitad** de tamaño.

**Cambio de relación de aspecto: ¿qué se mantiene y qué cambia?**
La **altura visible se mantiene fija**, porque la determina el Size. La que **cambia es la anchura**:

> anchura visible = 2 × Size × (ancho / alto)

| Relación de aspecto | Anchura visible (Size = 5) |
|---|---|
| 4:3 | 2 × 5 × 4/3 = **13,33 u** |
| 16:9 | 2 × 5 × 16/9 = **17,78 u** |
| 2560 × 1080 (≈ 2,37:1) | 2 × 5 × 2,37 = **23,70 u** |

**Si dos jugadores juegan con monitores distintos, ¿ven la misma cantidad de nivel?**
No. Ven la misma altura, pero el jugador con el monitor más panorámico ve más nivel en horizontal.

**¿En qué tipo de juego sería un problema?**
- En **juegos competitivos o multijugador** (lucha 2D, shooters 2D, RTS, MOBA), porque ver más terreno da ventaja: se ven antes los enemigos o los proyectiles.
- En juegos cuyo **diseño de nivel depende de lo que el jugador ve**: plataformas con saltos de fe, puzzles, sustos o enemigos que deben aparecer por sorpresa.

La solución habitual es fijar una relación de aspecto y añadir bandas negras (*letterbox / pillarbox*), por ejemplo con el *Crop Frame* de la **Pixel Perfect Camera**.

---

## Scripts

| Script | Ejercicios | Descripción |
|---|---|---|
| `PR3_MovimientoHorizontal.cs` | 3–7 | Movimiento en X con `Translate`, eje tipo `GetAxis` con el Input System, `flipX` y parámetro `isWalking` |
| `PR3_CambioAnimacionDistancia.cs` | 7 | Acumula la distancia recorrida y activa el parámetro `cansado` del Animator al superar la distancia de activación |
| `PR3_InfoSprite.cs` | 8–9 | Muestra el tamaño en píxeles, el PPU, el tamaño en unidades y el tamaño de un píxel en el mundo |
| `PR3_InfoCamara.cs` | 10 | Muestra la altura y la anchura visibles según el Size y el aspecto, y coloca marcadores en los bordes |

## Escenas

| Escena | Contenido |
|---|---|
| `PR3_Ej01-07_Personaje` | Atlas, animaciones, movimiento y Animator |
| `PR3_Ej08_PPU` | Pruebas de Pixels Per Unit |
| `PR3_Ej09_PPUMixto` | Sprites con distinto PPU |
| `PR3_Ej10_Camara` | Cámara ortográfica y relación de aspecto |