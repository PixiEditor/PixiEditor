# UI Compartida y Controles

## Proyectos incluidos

- `src/PixiDocks/src/PixiDocks.Core/` — lógica del sistema de paneles acoplables
- `src/PixiDocks/src/PixiDocks.Avalonia/` — controles Avalonia de docking
- `src/ColorPicker/src/ColorPicker.Models/` — lógica del selector de color
- `src/ColorPicker/src/ColorPicker.AvaloniaUI/` — controles Avalonia del color picker
- `src/PixiEditor.Zoombox/` — control de zoom del canvas
- `src/PixiEditor.UI.Common/` — controles, estilos, temas y localización compartidos

## Descripción

Este grupo reúne todos los controles de UI reutilizables que no dependen del dominio específico del editor de pixel art. Son componentes genéricos que podrían usarse en cualquier aplicación Avalonia: un sistema de paneles arrastrables, un selector de color, un control de zoom, y una biblioteca de controles y estilos compartidos.

Cada uno de estos proyectos es un submodulo Git independiente (PixiDocks, ColorPicker) o una biblioteca compartida dentro de la solución.

## Propósito

Evitar acoplamiento entre la UI genérica y la lógica del editor. Si mañana se necesita el color picker en otro proyecto, se puede reutilizar sin arrastrar todo PixiEditor.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `PixiDocks.Core/IDockable.cs` | Interfaz que todo panel acoplable debe implementar |
| `PixiDocks.Avalonia/DockableArea.cs` | Control Avalonia que aloja el sistema de docking |
| `ColorPicker.Models/ColorState.cs` | Estado del color seleccionado |
| `ColorPicker.AvaloniaUI/StandardColorPicker.cs` | Control visual del selector de color |
| `PixiEditor.Zoombox/Zoombox.cs` | Control de zoom y pan del canvas |
| `PixiEditor.UI.Common/` | Raíz de controles compartidos |

---

## Grupos de funcionalidades

### Sistema de Docking (PixiDocks)

El docking es lo que permite que los paneles del editor (capas, colores, timeline, nodos) se puedan mover, redimensionar, agrupar en tabs y sacar como ventanas flotantes.

**Carpeta:** `PixiDocks/src/PixiDocks.Core/`

Interfaces clave:
- `IDockable` — cualquier cosa que se pueda acoplar (un panel)
- `IDockableHost` — el contenedor que aloja dockables
- `IDockContext` — contexto que gestiona el estado del docking
- `IDockableTree` — el árbol de paneles (cómo están organizados)
- `IHostWindow` — ventana que puede recibir paneles flotantes

**Carpeta:** `PixiDocks/src/PixiDocks.Avalonia/`

Controles visuales: `DockableArea` (el área principal de docking), tabs arrastrables, divisores, y ventanas flotantes.

**Cómo se usa en PixiEditor:**

Los paneles del editor implementan `IDockable` a través de `DockableViewModel`:

```
PixiEditor/ViewModels/Dock/
  ├── LayersDockViewModel.cs       → Panel de capas
  ├── ColorPickerDockViewModel.cs  → Panel de colores
  ├── PaletteViewerDockViewModel.cs → Panel de paletas
  ├── TimelineDockViewModel.cs     → Línea de tiempo
  ├── NodeGraphDockViewModel.cs    → Editor de nodos
  └── ...
```

Las vistas correspondientes están en `PixiEditor/Views/Dock/`.

**Cómo agregar un panel acoplable nuevo:**

1. Crea un ViewModel en `ViewModels/Dock/MiPanelDockViewModel.cs` que herede de `DockableViewModel`.
2. Crea la vista en `Views/Dock/MiPanelDockView.axaml`.
3. Registra el panel en `LayoutManager.cs` (en `ViewModels/Dock/LayoutManager.cs`).
4. Opcionalmente, agrega una entrada en el menú "View" para mostrar/ocultar el panel usando un `OpenDockablesMenuBuilder`.

### Control de Zoom (Zoombox)

**Carpeta:** `src/PixiEditor.Zoombox/`

El Zoombox es el control que envuelve el canvas del editor y permite hacer zoom con la rueda del ratón, pan arrastrando, y encajar la imagen en la ventana. Es un control Avalonia que gestiona las transformaciones de coordenadas entre el espacio del documento y el espacio de la pantalla.

**Archivo principal:** `Zoombox.cs`

Funcionalidades:
- Zoom in/out con rueda del ratón (centrado en el cursor)
- Pan (arrastrar con clic medio o Space+clic)
- Encajar imagen en la vista
- Coordenadas de documento ↔ coordenadas de pantalla

### Selector de Color (ColorPicker)

**Carpeta:** `src/ColorPicker/`

Selector de color completo con rueda de color, sliders por canal (RGBA, HSV, HSL), campo hexadecimal, y soporte de gradientes.

**Modelos** (`ColorPicker.Models/`):
- `ColorState.cs` — estado actual del color con conversiones automáticas entre espacios de color
- Interfaces: `IColorStateStorage`, `IGradientStorage`, `ISecondColorStorage`

**Controles Avalonia** (`ColorPicker.AvaloniaUI/`):
- `StandardColorPicker` — selector completo (rueda + sliders + hex)
- `SquarePicker` — selector cuadrado de tono/saturación
- `AlphaSlider` — slider de transparencia
- `HueSlider` — slider de tono

**Cómo se usa en PixiEditor:**
- `Views/Dock/ColorPickerDockView.axaml` — panel acoplable de color
- `Views/Input/SmallColorPicker.axaml` — selector compacto para settings de herramientas

### Controles UI generales (PixiEditor.UI.Common)

**Carpeta:** `src/PixiEditor.UI.Common/`

Biblioteca de controles, estilos y utilidades Avalonia compartidas por toda la aplicación.

| Subcarpeta | Qué contiene |
|---|---|
| `Controls/` | Controles Avalonia personalizados (botones, inputs, etc.) |
| `Converters/` | Value converters para bindings XAML |
| `Behaviors/` | Behaviours Avalonia reutilizables |
| `Localization/` | Sistema de localización: `LocalizedString`, extensiones de marcado para i18n |
| `Styles/` | Estilos globales |
| `Themes/` | Temas de color |
| `Tweening/` | Animaciones de UI (easing, transiciones) |
| `Fonts/` | Fuente embebida `PixiPerfect.ttf` (iconos del editor) |
| `Helpers/` | Utilidades de UI |

**Localización:**

El sistema de localización permite usar strings traducidos tanto en código como en XAML:

```xml
<!-- En AXAML -->
<TextBlock Text="{ext:Loc MI_CLAVE_DE_TRADUCCION}" />
```

```csharp
// En código
LocalizedString texto = new LocalizedString("MI_CLAVE_DE_TRADUCCION");
```

Los archivos de idioma están en `PixiEditor/Data/Localization/Languages/`.
