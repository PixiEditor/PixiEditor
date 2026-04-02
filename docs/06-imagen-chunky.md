# Imagen por Chunks (ChunkyImageLib)

## Proyectos incluidos

- `src/ChunkyImageLib/` — biblioteca principal de imagen en tiles
- `src/ChunkyImageLibVis/` — herramienta de visualización/debug (WPF, solo para desarrollo)

## Descripción

ChunkyImageLib es la forma en que PixiEditor maneja imágenes en memoria. En lugar de guardar toda la imagen como un solo bitmap gigante, la divide en **chunks** (tiles) de tamaño fijo. Cuando el usuario pinta un trazo, solo se modifican los chunks que toca el pincel, no toda la imagen.

Las operaciones son **lazy**: se encolan como instrucciones pendientes y solo se calculan cuando se necesita el resultado (por ejemplo, al renderizar). Esto permite:
- Trabajar con imágenes muy grandes sin consumir toda la RAM
- Renderizar solo los chunks visibles en la pantalla
- Paralelizar operaciones por chunk
- Mantener dos estados (committed y latest) para undo/redo eficiente

## Propósito

Hacer eficiente la edición de imágenes grandes. Sin este sistema, cada trazo de pincel requeriría copiar toda la imagen para el undo, lo cual sería prohibitivo para imágenes de alta resolución.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `ChunkyImage.cs` | La imagen principal — punto de acceso a todas las operaciones |
| `IReadOnlyChunkyImage.cs` | Interfaz de solo lectura (la usa el renderizador) |
| `Chunk.cs` | Un tile individual con su superficie de dibujo |
| `ChunkPool.cs` | Pool de chunks reutilizables para evitar presión del GC |

---

## Grupos de funcionalidades

### La imagen y sus chunks

**Archivo principal:** `ChunkyImage.cs`

Una `ChunkyImage` es una colección de chunks organizados en una grilla. Cada chunk tiene un tamaño fijo (generalmente 64x64 o 128x128 píxeles). La imagen expone métodos de dibujo que internamente determinan qué chunks se ven afectados y solo trabajan sobre esos.

La imagen tiene dos estados:
- **Committed** — el estado guardado/confirmado (es el que se usa para undo)
- **Latest** — el estado actual con las operaciones pendientes aplicadas

Cuando el usuario confirma un trazo (suelta el ratón), los cambios pasan de "latest" a "committed".

**Archivo:** `Chunk.cs`

Un chunk individual que contiene una `DrawingSurface` de Drawie. Los chunks se crean bajo demanda: si un área de la imagen nunca se ha tocado, no existe chunk para esa zona (ahorro de memoria).

**Archivo:** `ChunkPool.cs`

Pool de objetos para reutilizar chunks. Cuando un chunk se libera, vuelve al pool en lugar de destruirse. Esto reduce la presión sobre el garbage collector.

**Archivo:** `CommittedChunkStorage.cs`

Almacena los datos de los chunks confirmados de forma comprimida, para undo/redo eficiente.

### Operaciones de dibujo

**Carpeta:** `Operations/`

Contiene las operaciones que se pueden aplicar sobre chunks: pintar, borrar, rellenar, dibujar formas, aplicar filtros a nivel de chunk, etc. Cada operación recibe un contexto de dibujo y modifica solo los chunks afectados.

### Tipos de datos auxiliares

**Carpeta:** `DataHolders/`

Estructuras auxiliares para pasar datos entre operaciones, como información de regiones afectadas o buffers temporales.

### Interfaz de solo lectura

**Archivo:** `IReadOnlyChunkyImage.cs`

Esta interfaz es crucial: expone la imagen de forma que se puede leer (para renderizar, para calcular bounds) pero no modificar. El renderizador del viewport y los nodos del grafo trabajan con esta interfaz.

Métodos clave:
- Dibujar chunks en una superficie de destino
- Obtener el color de un píxel específico
- Calcular los bounds (rectángulo mínimo que contiene píxeles no transparentes)
- Listar qué chunks tienen contenido
