# Sistema de Herramientas

## Proyectos incluidos

- `src/PixiEditor/ViewModels/Tools/` — ViewModels de herramientas
- `src/PixiEditor/ViewModels/SubViewModels/ToolsViewModel.cs` — gestor de herramientas
- `src/PixiEditor/Models/DocumentModels/UpdateableChangeExecutors/` — lógica de modificación del documento
- `src/PixiEditor/Models/BrushEngine/` — motor de pinceles
- `src/PixiEditor/Views/Overlays/` — overlays visuales sobre el canvas
- `src/PixiEditor/Views/Main/Tools/` — UI de la barra de herramientas

## Descripción

Las herramientas (lápiz, borrador, selección, formas, texto, etc.) son la forma principal en que el usuario interactúa con el canvas. Cada herramienta se implementa en **tres capas** independientes:

1. **ToolViewModel** — la herramienta en sí: su icono, nombre, atajos, configuración (tamaño, opacidad, etc.)
2. **Executor** — la lógica que traduce los eventos del puntero (clic, arrastrar, soltar) en acciones sobre el documento
3. **Overlay** — la representación visual sobre el canvas (contorno del pincel, marcha de hormigas de la selección, handles de transformación)

Esta separación permite que cada capa sea independiente y reutilizable. Por ejemplo, el overlay de transformación se usa tanto con la herramienta de mover como al pegar contenido.

## Propósito

Encapsular cada herramienta como una unidad independiente y extensible. Agregar una herramienta nueva no requiere modificar las herramientas existentes.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `ViewModels/SubViewModels/ToolsViewModel.cs` | Gestiona la herramienta activa, el cambio entre herramientas |
| `ViewModels/Tools/ToolViewModel.cs` | Clase base abstracta de toda herramienta |
| `ViewModels/Tools/Tools/` | Una implementación por herramienta |
| `Models/DocumentModels/UpdateableChangeExecutors/` | Executors que conectan herramientas con el documento |
| `Models/Handlers/Tools/` | Interfaces para cada herramienta |

---

## Grupos de funcionalidades

### ViewModels de herramientas

Cada herramienta es un ViewModel que define su configuración y comportamiento.

**Carpeta:** `PixiEditor/ViewModels/Tools/Tools/`

| Herramienta | Clase | Descripción |
|---|---|---|
| Lápiz | `PenToolViewModel` | Dibuja trazos libres con el pincel activo |
| Borrador | `EraserToolViewModel` | Borra píxeles (hereda de `BrushBasedToolViewModel`) |
| Relleno | `FloodFillToolViewModel` | Rellena áreas conectadas del mismo color |
| Mover | `MoveToolViewModel` | Mueve la selección o capa activa |
| Selección rectangular | `SelectToolViewModel` | Selección rectangular o cuadrada |
| Lazo | `LassoToolViewModel` | Selección a mano alzada |
| Varita mágica | `MagicWandToolViewModel` | Selección por color similar |
| Cuentagotas | `ColorPickerToolViewModel` | Toma el color de un píxel |
| Texto | `TextToolViewModel` | Agrega y edita texto |
| Zoom | `ZoomToolViewModel` | Hace zoom en el canvas |
| Línea raster | `RasterLineToolViewModel` | Dibuja líneas de píxeles |
| Rectángulo raster | `RasterRectangleToolViewModel` | Dibuja rectángulos de píxeles |
| Elipse raster | `RasterEllipseToolViewModel` | Dibuja elipses de píxeles |
| Línea vectorial | `VectorLineToolViewModel` | Crea líneas vectoriales |
| Rectángulo vectorial | `VectorRectangleToolViewModel` | Crea rectángulos vectoriales |
| Elipse vectorial | `VectorEllipseToolViewModel` | Crea elipses vectoriales |
| Path vectorial | `VectorPathToolViewModel` | Crea paths vectoriales con puntos de control |
| Mover viewport | `MoveViewportToolViewModel` | Pan del canvas (mano) |
| Rotar viewport | `RotateViewportToolViewModel` | Rota la vista del canvas |

**Jerarquía de herencia:**

```
ToolViewModel (base abstracta)
  ├── BrushBasedToolViewModel (herramientas basadas en pincel)
  │    ├── PenToolViewModel
  │    └── EraserToolViewModel
  ├── ShapeTool (herramientas de forma)
  │    ├── RasterLineToolViewModel
  │    ├── RasterRectangleToolViewModel
  │    └── RasterEllipseToolViewModel
  ├── MoveToolViewModel
  ├── SelectToolViewModel
  ├── LassoToolViewModel
  ├── MagicWandToolViewModel
  ├── FloodFillToolViewModel
  ├── ColorPickerToolViewModel
  ├── TextToolViewModel
  ├── ZoomToolViewModel
  ├── VectorEllipseToolViewModel
  ├── VectorRectangleToolViewModel
  ├── VectorLineToolViewModel
  ├── VectorPathToolViewModel
  ├── MoveViewportToolViewModel
  └── RotateViewportToolViewModel
```

### Configuración de herramientas (Toolbars y Settings)

Cada herramienta puede tener su propia barra de configuración (toolbar) que aparece en la parte superior cuando la herramienta está activa.

**Carpeta:** `ViewModels/Tools/ToolSettings/`

- `Settings/` — tipos de configuración: `BoolSetting`, `FloatSetting`, `ColorSetting`, `EnumSetting`, etc.
- `Toolbars/` — toolbars predefinidas: `BrushToolbar`, `ShapeToolbar`, `TextToolbar`, `BasicToolbar`, etc.

**Vistas correspondientes:**
- `Views/Tools/ToolSettings/Settings/` — una vista por tipo de setting (slider, checkbox, color picker, etc.)
- `Views/Main/Tools/Toolbar.axaml` — el contenedor de la toolbar activa

### Executors (cómo las herramientas modifican el documento)

Los executors son la "conexión" entre la herramienta y el sistema de cambios del documento. Reciben eventos de puntero (inicio de trazo, movimiento, fin de trazo) y lanzan Actions al ChangeableDocument.

**Carpeta:** `Models/DocumentModels/UpdateableChangeExecutors/`

Cada herramienta tiene su executor:
- `PenToolExecutor.cs` — convierte movimientos del puntero en trazos de pincel
- `EraserToolExecutor.cs` — similar al pen pero borra
- `FloodFillToolExecutor.cs` — calcula el área a rellenar y la llena
- `DrawableShapeToolExecutor.cs` — dibuja formas mientras se arrastra
- `SelectToolExecutor.cs`, `LassoToolExecutor.cs`, `MagicWandToolExecutor.cs` — diferentes modos de selección
- `TransformSelectedExecutor.cs` — mover y transformar la selección activa
- `LineExecutor.cs` — dibujar líneas
- `TextToolExecutor.cs` — editar texto

### Motor de pinceles

**Carpeta:** `Models/BrushEngine/`

El motor de pinceles gestiona cómo se aplican los trazos del pincel al canvas. Incluye la interpolación entre puntos del trazo, la presión del stylus, y las propiedades del pincel (tamaño, opacidad, spacing, etc.).

**Archivos relacionados:**
- `ViewModels/SubViewModels/BrushesViewModel.cs` — biblioteca de pinceles disponibles
- `ViewModels/BrushSystem/BrushViewModel.cs` — un pincel individual
- `Data/Brushes/` — texturas y definiciones de pinceles
- `Data/BrushTools/` — configuraciones de herramientas de pincel

### Overlays sobre el canvas

Los overlays son elementos visuales que se dibujan sobre el canvas pero que no forman parte del documento. Sirven para dar feedback visual al usuario.

**Carpeta:** `Views/Overlays/`

| Overlay | Qué muestra |
|---|---|
| `BrushShapeOverlay/` | Contorno del pincel que sigue al cursor |
| `TransformOverlay/` | Handles de transformación (mover, escalar, rotar) |
| `SelectionOverlay/` | "Marcha de hormigas" alrededor de la selección |
| `SymmetryOverlay.cs` | Guías de simetría (horizontal, vertical) |
| `LineToolOverlay/` | Preview de la línea mientras se dibuja |
| `PathOverlay/` | Puntos de control del path vectorial |
| `TextOverlay/` | Cursor y caja de edición de texto |
| `GridLinesOverlay.cs` | Cuadrícula de píxeles |
| `Handles/` | Handles genéricos (puntos de arrastre, rotación) |

### UI de la barra de herramientas

**Archivos clave:**
- `Views/Main/Tools/ToolsPicker.axaml` — barra lateral izquierda con iconos de herramientas
- `Views/Main/Tools/Toolbar.axaml` — barra superior con opciones de la herramienta activa
- `Views/Main/Tools/ToolPickerButton.axaml` — un botón individual de herramienta

---

## Cómo agregar una herramienta nueva

1. **Crea el ViewModel** en `ViewModels/Tools/Tools/MiHerramientaToolViewModel.cs`:

```csharp
[Tool(Key.M)] // atajo de teclado
internal class MiHerramientaToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MI_HERRAMIENTA";
    public override Type[]? SupportedLayerTypes => [typeof(ImageLayerNode)];

    // Configuración de la toolbar (opcional)
    public override Type? ToolbarType => typeof(BasicToolbar);
}
```

2. **Crea la interfaz de handler** en `Models/Handlers/Tools/IMiHerramientaToolHandler.cs`:

```csharp
public interface IMiHerramientaToolHandler : IToolHandler { }
```

3. **Registra en DI** en `ServiceCollectionHelpers.cs`:

```csharp
.AddTool<IMiHerramientaToolHandler, MiHerramientaToolViewModel>()
```

4. **Crea el Executor** en `Models/DocumentModels/UpdateableChangeExecutors/MiHerramientaExecutor.cs` para definir cómo modifica el documento.

5. **Crea el Overlay** (opcional) en `Views/Overlays/` si necesitas feedback visual sobre el canvas.

6. **Crea el icono** en `Images/Tools/` (SVG o PNG).
