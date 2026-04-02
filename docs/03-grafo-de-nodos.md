# Grafo de Nodos

## Proyectos incluidos

- `src/PixiEditor.ChangeableDocument/Changeables/Graph/` — definición lógica de nodos y grafo
- `src/PixiEditor/ViewModels/Document/Nodes/` — ViewModels para la UI del editor de nodos
- `src/PixiEditor/Views/Nodes/` — vistas del editor de nodos
- `src/PixiEditor/ViewModels/Dock/NodeGraphDockViewModel.cs` — panel acoplable del grafo

## Descripción

El sistema de composición de PixiEditor se basa en un grafo de nodos, similar a Blender o Substance Designer. En lugar de una pila plana de capas, cada capa es un **nodo** que puede conectarse con otros nodos para producir efectos, filtros, transformaciones y composiciones complejas.

El grafo tiene dos caras: la **lógica** (en ChangeableDocument, donde los nodos procesan datos) y la **visual** (en PixiEditor, donde el usuario ve y edita las conexiones). Ambas caras se mantienen sincronizadas a través del sistema de ChangeInfos.

Cuando el usuario dibuja en una capa, la modificación va al nodo correspondiente en el grafo, y luego el grafo se re-evalúa para producir la imagen final.

## Propósito

Permitir composición no-destructiva: los efectos, filtros y transformaciones son nodos que se pueden reorganizar, conectar y desconectar sin perder información. Esto es mucho más potente que el modelo tradicional de capas apiladas.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `ChangeableDocument/Changeables/Graph/NodeGraph.cs` | El grafo en sí — lista de nodos y conexiones |
| `ChangeableDocument/Changeables/Graph/Nodes/Node.cs` | Clase base abstracta de todo nodo |
| `PixiEditor/ViewModels/Document/NodeGraphViewModel.cs` | ViewModel del editor visual de nodos |
| `PixiEditor/Views/Nodes/` | Vistas AXAML del editor de nodos |
| `PixiEditor/ViewModels/Dock/NodeGraphDockViewModel.cs` | Panel acoplable que aloja el editor |

---

## Grupos de funcionalidades

### Nodos de documento (lógica)

Cada nodo hereda de `Node` y define inputs, outputs y la lógica de procesamiento.

**Carpeta:** `ChangeableDocument/Changeables/Graph/Nodes/`

| Subcarpeta/Nodo | Qué hace |
|---|---|
| `ImageLayerNode.cs` | Capa de imagen raster (la más común) |
| `FolderNode.cs` | Carpeta que agrupa nodos hijos |
| `OutputNode.cs` | Nodo de salida final (la imagen que se ve) |
| `MergeNode.cs` | Combina dos imágenes |
| `FilterNodes/` | Filtros de imagen: blur, sharpen, color adjust, etc. |
| `Effects/` | Efectos visuales |
| `Vectors/` | Nodos vectoriales: VectorLayerNode, etc. |
| `Animable/` | Nodos con soporte de animación: RasterCelNode, etc. |
| `Brushes/` | Nodos de pincel |
| `CombineSeparate/` | Combinar/separar canales de color |
| `Calculations/` | Operaciones matemáticas sobre valores |
| `Matrix/` | Transformaciones matriciales |
| `Generators/` | Generadores de imagen: ruido, formas, etc. |

**Cómo agregar un nodo nuevo:**

1. Crea una clase en la subcarpeta temática dentro de `Nodes/`.
2. Hereda de `Node`.
3. Decora la clase con `[NodeInfo("NombreUnico")]`.
4. Define las propiedades de entrada con `[Input]` y las de salida con `[Output]`.
5. Implementa el método `OnExecute()` donde va la lógica de procesamiento.

Ejemplo simplificado:

```csharp
// En ChangeableDocument/Changeables/Graph/Nodes/FilterNodes/MiFiltro.cs
[NodeInfo("MiFiltro")]
internal class MiFiltroNode : Node
{
    // Entrada: la imagen a procesar
    [Input] public Texture InputImage { get; set; }

    // Parámetro configurable
    [Input] public double Intensidad { get; set; } = 0.5;

    // Salida: la imagen procesada
    [Output] public Texture OutputImage { get; set; }

    protected override Texture? OnExecute(RenderingContext context)
    {
        // Tu lógica de filtro aquí
        // Usa InputImage y Intensidad para generar OutputImage
    }
}
```

6. Crea el ViewModel correspondiente en `PixiEditor/ViewModels/Document/Nodes/` (ver sección siguiente).
7. Crea la SerializationFactory en `PixiEditor/Models/Serialization/Factories/` para que el nodo se guarde en archivos `.pixi`.

### ViewModels de nodos (UI del editor de nodos)

Cada nodo del documento tiene un ViewModel espejo que la UI usa para mostrarlo en el editor de nodos.

**Carpeta:** `PixiEditor/ViewModels/Document/Nodes/`

Contiene ~80 archivos de NodeViewModels, cada uno correspondiendo a un tipo de nodo. Ejemplos: `ImageLayerNodeViewModel.cs`, `FolderNodeViewModel.cs`, `BlurNodeViewModel.cs`, etc.

**Archivos base:**
- `NodeViewModel.cs` — base de todo ViewModel de nodo
- `NodePropertyViewModel.cs` — propiedad individual de un nodo (input u output)
- `NodeConnectionViewModel.cs` — una conexión entre el output de un nodo y el input de otro
- `NodeFrameViewModel.cs` — agrupador visual de nodos en el editor

**Propiedades de nodos** en `ViewModels/Nodes/Properties/`:
- `ColorPropertyViewModel`, `BoolPropertyViewModel`, `DoublePropertyViewModel`, etc.
- Cada tipo de propiedad tiene su vista asociada en `Views/Nodes/Properties/`

### Vista del editor de nodos

**Carpeta:** `PixiEditor/Views/Nodes/`

- `NodeGraphView.cs` — canvas interactivo donde se muestran los nodos y conexiones
- `NodeView.cs` — representación visual de un nodo individual
- `ConnectionLine.cs` — línea curva entre nodos
- `NodePicker.cs` — buscador para agregar nodos nuevos al grafo (aparece al hacer clic derecho)

El editor de nodos vive dentro de un panel acoplable (`NodeGraphDockViewModel`) que se puede mover, redimensionar y ocultar como cualquier otro panel del editor.

### Blackboard (variables del grafo)

El Blackboard es un sistema de variables globales que cualquier nodo del grafo puede leer. Funciona como un diccionario de valores que se pueden actualizar desde la UI.

**Archivos clave:**
- `PixiEditor/ViewModels/Document/Blackboard/BlackboardViewModel.cs` — gestión de variables
- `PixiEditor/ViewModels/Document/Blackboard/VariableViewModel.cs` — una variable individual
- `PixiEditor/Views/Blackboard/BlackboardView.axaml` — panel visual de variables
- `ChangeableDocument/Changeables/Graph/Nodes/BlackboardVariableValueNode.cs` — nodo que lee una variable del blackboard

### Conexiones y tipos de datos entre nodos

Los nodos se conectan a través de "sockets" tipados. Un output de tipo `Texture` solo puede conectarse a un input de tipo `Texture`. Los tipos de datos comunes son:

- `Texture` — imagen/superficie
- `Color` — un color RGBA
- `double`, `float`, `int` — valores numéricos
- `VecD` — vector 2D (posición, tamaño)
- `bool` — valor booleano
- `string` — texto
- `Paintable` — un color o gradiente que puede usarse para pintar
