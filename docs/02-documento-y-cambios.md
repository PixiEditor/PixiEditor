# Documento y Sistema de Cambios (ChangeableDocument)

## Proyectos incluidos

- `src/PixiEditor.ChangeableDocument/` — motor de documento con CQRS y undo/redo
- `src/PixiEditor.ChangeableDocument.Gen/` — generador de código Roslyn para acciones

## Descripción

Este proyecto es el núcleo del modelo de datos del editor. Toda modificación al documento (pintar un píxel, mover una capa, cambiar un color) pasa por aquí como una "acción" que genera un "cambio". Cada cambio sabe cómo aplicarse y cómo revertirse, lo que hace posible el undo/redo completo.

La arquitectura sigue el patrón CQRS (Command Query Responsibility Segregation): las acciones son comandos que modifican el estado, y las ChangeInfos son las notificaciones que informan a la UI qué parte del documento cambió. La UI nunca modifica el documento directamente — siempre envía una acción.

El documento internamente se representa como un **grafo de nodos** donde cada capa, carpeta y efecto es un nodo conectado. Esto permite composición no-destructiva avanzada (ver [03-grafo-de-nodos](03-grafo-de-nodos.md)).

## Propósito

Aislar toda la lógica de modificación del documento de la UI. Esto garantiza:
- Undo/redo coherente en todas las operaciones
- Que la UI solo pueda modificar el documento a través de acciones bien definidas
- Que el estado del documento sea predecible y testeable

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `Changeables/Document.cs` | Estado completo del documento (nodos, selección, tamaño, etc.) |
| `IReadOnlyDocument` | Interfaz de solo lectura que expone el estado del documento a la UI |
| `Actions/` | Carpeta con todas las acciones posibles |
| `Changes/` | Carpeta con todas las implementaciones de cambios |

---

## Grupos de funcionalidades

### Acciones (Actions) — "Quiero hacer X"

Las acciones son mensajes que la app envía al documento para pedir una modificación. No contienen lógica — solo datos.

**Carpeta:** `PixiEditor.ChangeableDocument/Actions/`

Hay varios tipos de acciones:

- **`IMakeChangeAction`** — acción instantánea, se ejecuta de una vez. Ejemplo: llenar con color, borrar capa.
- **`IStartOrUpdateChangeAction`** — acción que se actualiza continuamente (como al arrastrar un pincel). Se llama múltiples veces mientras el usuario arrastra.
- **`IEndChangeAction`** — señal de que una acción continua terminó (el usuario soltó el ratón).
- **`IChangeInfo`** — notificación de vuelta a la UI sobre qué cambió.

Subcarpetas temáticas:
- `Actions/Undo/` — acciones de deshacer y rehacer
- Las demás acciones se generan automáticamente por el source generator a partir de los Changes.

### Cambios (Changes) — "Así se modifica el documento"

Cada Change implementa la lógica real de modificación, incluyendo cómo revertirla.

**Carpeta:** `PixiEditor.ChangeableDocument/Changes/`

| Subcarpeta | Qué modifica |
|---|---|
| `Drawing/` | Operaciones de dibujo: pintar, borrar, flood fill, operaciones de píxeles |
| `Structure/` | Estructura del documento: agregar/mover/eliminar capas y carpetas |
| `NodeGraph/` | Conexiones entre nodos, propiedades de nodos |
| `Animation/` | Keyframes, celdas de animación |
| `Selection/` | Selección activa (agregar, quitar, invertir, etc.) |
| `Text/` | Capas de texto (contenido, fuente, estilo) |
| `Vectors/` | Formas vectoriales (paths, transformaciones) |
| `Properties/` | Propiedades genéricas de nodos |
| `Root/` | Tamaño del documento, capa de referencia, simetría |

**Cómo agregar una operación nueva:**

1. Crea un archivo en la subcarpeta temática dentro de `Changes/`. Por ejemplo, `Changes/Drawing/MiOperacion_Change.cs`.
2. La clase debe tener un constructor decorado con `[GenerateMakeChangeAction]` o `[GenerateUpdateableChangeAction]`.
3. El source generator (`ChangeableDocument.Gen`) creará automáticamente la Action correspondiente.

Ejemplo simplificado:

```csharp
// En Changes/Drawing/MiOperacion_Change.cs
internal class MiOperacion_Change : Change
{
    private readonly Guid layerId;
    private readonly Color color;

    // El generador creará una Action con estos mismos parámetros
    [GenerateMakeChangeAction]
    public MiOperacion_Change(Guid layerId, Color color)
    {
        this.layerId = layerId;
        this.color = color;
    }

    // Aplica el cambio
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(
        Document target, bool firstApply, out bool ignoreInUndo)
    {
        // Lógica de modificación del documento
    }

    // Revierte el cambio (para undo)
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        // Lógica de reversión
    }
}
```

### Estado del documento (Changeables)

Los objetos vivos que representan el estado actual del documento.

**Carpeta:** `PixiEditor.ChangeableDocument/Changeables/`

| Archivo/Carpeta | Qué representa |
|---|---|
| `Document.cs` | Raíz del documento: contiene el grafo, la selección, el tamaño |
| `Graph/NodeGraph.cs` | El grafo de nodos completo |
| `Graph/Nodes/` | Todos los tipos de nodo (~40+ implementaciones) |
| `Animations/` | Datos de keyframes, celdas, frames |
| `Selection.cs` | Selección activa como imagen binaria |

### Notificaciones de cambio (ChangeInfos)

DTOs que informan a la UI qué parte del documento cambió. La UI escucha estos cambios y actualiza solo lo necesario.

**Carpeta:** `PixiEditor.ChangeableDocument/ChangeInfos/`

Cada ChangeInfo corresponde a un tipo de modificación: `LayerImageChanged`, `NodePropertyChanged`, `SelectionChanged`, `DocumentSizeChanged`, etc.

### Renderizado del grafo

**Carpeta:** `PixiEditor.ChangeableDocument/Rendering/`

Aquí se encuentra la lógica que evalúa el grafo de nodos para producir la imagen final. Cuando el grafo cambia, el renderizador recalcula solo los nodos afectados.

### Generador de código (ChangeableDocument.Gen)

**Carpeta:** `src/PixiEditor.ChangeableDocument.Gen/`

Este Roslyn source generator lee los constructores decorados con `[GenerateMakeChangeAction]` y `[GenerateUpdateableChangeAction]` y genera automáticamente las clases de Action correspondientes. Esto elimina el boilerplate de crear una Action por cada Change.
