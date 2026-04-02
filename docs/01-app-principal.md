# Aplicación Principal (PixiEditor)

## Proyectos incluidos

- `src/PixiEditor/` — biblioteca central con toda la UI, ViewModels y Models
- `src/PixiEditor.Desktop/` — ejecutable de escritorio (Windows, Linux, macOS)
- `src/PixiEditor.Browser/` — ejecutable para WebAssembly (navegador)

## Descripción

Este es el corazón de la aplicación. Aquí vive toda la interfaz de usuario construida con Avalonia (un framework similar a WPF pero multiplataforma), la lógica de presentación (ViewModels) y los modelos que coordinan la comunicación entre la UI y el sistema de documentos.

La arquitectura es MVVM: cada vista (archivo `.axaml`) tiene un ViewModel asociado que gestiona su estado y lógica. Los ViewModels se comunican entre sí a través de un ViewModel raíz llamado `ViewModelMain`, que actúa como punto de coordinación central.

El proyecto `PixiEditor.Desktop` solo contiene un `Program.cs` que arranca Avalonia. El proyecto `PixiEditor.Browser` hace lo mismo pero para WebAssembly. Toda la lógica real está en `src/PixiEditor/`.

## Propósito

Separar la aplicación de su host. `PixiEditor` es una biblioteca que puede ser hospedada tanto en escritorio como en el navegador. Los proyectos Desktop y Browser son solo "lanzadores" que configuran el entorno apropiado.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `PixiEditor.Desktop/Program.cs` | Arranca Avalonia con soporte para Vulkan/OpenGL |
| `PixiEditor/App.axaml.cs` | Inicializa la aplicación Avalonia y crea el `ClassicDesktopEntry` |
| `PixiEditor/Initialization/ClassicDesktopEntry.cs` | **El bootstrapper real**: detecta OS, carga plataforma, carga extensiones, configura el contenedor DI, crea la ventana principal |
| `PixiEditor/Helpers/ServiceCollectionHelpers.cs` | **Registro DI central**: aquí se registran TODOS los ViewModels, herramientas, tipos de archivo, parsers de paleta y builders de menú |
| `PixiEditor/ViewModels/ViewModelMain.cs` | ViewModel raíz que posee todos los sub-ViewModels |

---

## Grupos de funcionalidades

### Arranque y configuración de servicios (DI)

Cuando necesites registrar un servicio nuevo, un ViewModel nuevo o una herramienta nueva, este es el lugar.

**Archivos clave:**
- `PixiEditor/Initialization/ClassicDesktopEntry.cs` — flujo de arranque
- `PixiEditor/Helpers/ServiceCollectionHelpers.cs` — método `AddPixiEditor()` que registra todo

El flujo de arranque es:

1. `ClassicDesktopEntry.Start()` se ejecuta al iniciar
2. Llama a `InitOperatingSystem()` que registra el OS según la compilación (`WINDOWS`, `LINUX`, `MACOS`)
3. Llama a `InitApp()` que:
   - Registra la plataforma (Steam, MSStore o Standalone según los símbolos de compilación)
   - Carga las extensiones desde disco
   - Crea el `ServiceCollection`, registra todo con `AddPixiEditor()` y construye el `ServiceProvider`
4. Llama a `Load()` que invoca `ViewModelMain.Setup(services)` y crea la `MainWindow`

Para agregar un servicio nuevo, edita `ServiceCollectionHelpers.cs` y agrega tu registro dentro de `AddPixiEditor()`. Todos los servicios se registran como `Singleton`.

### ViewModel raíz y sub-ViewModels

El `ViewModelMain` es el ViewModel principal. No implementa lógica de dominio directamente, sino que delega en sub-ViewModels especializados:

**Archivo:** `PixiEditor/ViewModels/ViewModelMain.cs`

| Sub-ViewModel | Responsabilidad |
|---|---|
| `FileSubViewModel` | Abrir, guardar, exportar archivos |
| `ToolsSubViewModel` | Herramienta activa, cambio de herramienta |
| `ColorsSubViewModel` | Color primario/secundario, historial de colores |
| `LayersSubViewModel` | Gestión de capas en la UI |
| `UndoSubViewModel` | Deshacer y rehacer |
| `SelectionSubViewModel` | Selección activa |
| `ClipboardSubViewModel` | Copiar, pegar, cortar |
| `ViewportSubViewModel` | Opciones de vista (grilla, snap, zoom) |
| `AnimationsSubViewModel` | Control de animación |
| `BrushesSubViewModel` | Biblioteca de pinceles |
| `NodeGraphManager` | Apertura y gestión del editor de nodos |
| `DocumentManagerSubViewModel` | Documentos abiertos, documento activo |
| `WindowSubViewModel` | Gestión de ventanas y viewports |
| `SearchSubViewModel` | Búsqueda de comandos (Ctrl+K) |
| `UpdateSubViewModel` | Actualizaciones de la app |
| `UserViewModel` | Estado del usuario autenticado |
| `ExtensionsSubViewModel` | Gestión de extensiones |
| `LayoutSubViewModel` | Layout de paneles del editor |
| `DebugSubViewModel` | Herramientas de depuración |
| `DiscordViewModel` | Integración con Discord Rich Presence |

Cada sub-ViewModel hereda de `SubViewModel<ViewModelMain>` y tiene acceso al propietario (`Owner`) que es el `ViewModelMain`.

En `ViewModelMain.Setup(services)` se inicializan todos estos sub-ViewModels obteniéndolos del contenedor DI.

### Conexión entre Views y ViewModels

**Archivo:** `PixiEditor/ViewLocator.cs`

El `ViewLocator` implementa `IDataTemplate` de Avalonia y resuelve la vista correspondiente a cada ViewModel de dos formas:

1. **Por convención de nombres:** reemplaza `"ViewModel"` por `"View"` en el nombre completo del tipo. Ejemplo: `ColorsViewModel` → `ColorsView`.
2. **Por mapa explícito** para excepciones:
   - `ViewportWindowViewModel` → `DocumentTemplate`
   - `LazyViewportWindowViewModel` → `LazyDocumentTemplate`
   - `LayersDockViewModel` → `LayersManager`

Además, en los archivos AXAML se puede acceder a cualquier sub-ViewModel directamente usando la extensión de marcado `MainVM`:

```xml
<!-- Acceder al FileViewModel desde cualquier vista -->
DataContext="{local:MainVM FileSubViewModel}"

<!-- Acceder al ToolsViewModel -->
ItemsSource="{Binding Tools, Source={local:MainVM ToolsSubViewModel}}"
```

### Vista principal y ventana

La estructura visual de la aplicación se organiza así:

```
MainWindow.axaml                    ← Ventana raíz (barra de título, chrome)
  └── MainView.axaml               ← Contenido principal
       ├── MainTitleBar.axaml       ← Barra de título personalizada
       ├── ActionDisplayBar.axaml   ← Barra de acción activa (arriba)
       ├── ToolsPicker.axaml        ← Panel lateral izquierdo de herramientas
       ├── Toolbar.axaml            ← Barra de opciones de la herramienta activa
       ├── DockableArea              ← Área de paneles acoplables (PixiDocks)
       │    ├── Viewport.axaml      ← Lienzo principal de dibujo
       │    ├── LayersManager       ← Panel de capas
       │    ├── ColorPickerDock     ← Panel de colores
       │    ├── TimelineDock        ← Línea de tiempo de animación
       │    ├── NodeGraphDock       ← Editor de nodos
       │    └── PaletteViewerDock   ← Paletas de colores
       └── CommandSearch            ← Búsqueda de comandos (oculto por defecto)
```

**Archivos clave:**
- `Views/MainWindow.axaml` — ventana raíz
- `Views/MainView.axaml` — layout principal
- `Views/Main/ViewportControls/Viewport.axaml` — el canvas donde se dibuja
- `Views/Main/Tools/ToolsPicker.axaml` — barra lateral de herramientas
- `Views/Main/Tools/Toolbar.axaml` — opciones de la herramienta seleccionada

### Sistema de comandos y atajos de teclado

El sistema de comandos permite que cualquier acción del editor sea invocable por nombre, asignable a un atajo de teclado y buscable desde la paleta de comandos.

**Archivos clave:**
- `Models/Commands/CommandController.cs` — registro y despacho de todos los comandos
- `Models/Controllers/ShortcutController.cs` — mapeo de teclas a comandos

Para crear un comando nuevo, simplemente decora un método en cualquier ViewModel con el atributo `[Command.Basic]`:

```csharp
// En cualquier SubViewModel, por ejemplo FileViewModel.cs:
[Command.Basic("PixiEditor.File.MiComando", "Mi Comando", "Descripción del comando")]
public void MiComando()
{
    // Lógica del comando
}
```

El `CommandController.Init(services)` escanea todos los tipos registrados en el DI buscando estos atributos y los registra automáticamente. No necesitas registrar nada manualmente.

Para que un comando tenga un evaluador de habilitación (CanExecute):

```csharp
[Evaluator.CanExecute("PixiEditor.File.MiComando")]
public bool PuedoEjecutar() => DocumentManagerSubViewModel.ActiveDocument != null;
```

### Menú principal

El menú se construye de forma declarativa mediante `MenuItemBuilder`s.

**Archivos clave:**
- `ViewModels/Menu/MenuBarViewModel.cs` — inicializa el menú completo
- `ViewModels/Menu/MenuBuilders/` — builders individuales por sección

Builders existentes: `RecentFilesMenuBuilder`, `FileExitMenuBuilder`, `SymmetryMenuBuilder`, `OpenDockablesMenuBuilder`, `ToggleGridLinesMenuBuilder`, `ToggleSnappingMenuBuilder`, `ToggleHighResPreviewMenuBuilder`.

Para agregar un builder de menú nuevo, crea una clase que herede de `MenuItemBuilder`, regístrala en `ServiceCollectionHelpers.AddMenuBuilders()`.

### Diálogos y popups

Hay dos capas para los diálogos:

1. **Wrappers programáticos** en `Models/Dialogs/` — se usan desde los ViewModels para abrir un diálogo:
   - `ConfirmationDialog.Show(message, title)` — diálogo de Sí/No/Cancelar
   - `OptionsDialog<T>` — diálogo genérico con opciones
   - `NoticeDialog` — diálogo de aviso

2. **Vistas AXAML** en `Views/Dialogs/` — la UI visual de cada popup:
   - `NewFilePopup.axaml` — crear documento nuevo
   - `ExportFilePopup.axaml` — exportar archivo
   - `ResizeDocumentPopup.axaml` — redimensionar documento
   - `ShortcutsPopup.axaml` — editor de atajos de teclado
   - `AboutPopup.axaml` — acerca de
   - `OnboardingDialog.axaml` — tutorial de bienvenida
   - `CrashReportDialog.axaml` — reporte de errores

Para crear un diálogo nuevo: crea un `.axaml` en `Views/Dialogs/`, opcionalmente crea un wrapper en `Models/Dialogs/`, y ábrelo desde el ViewModel correspondiente.

### Manejo de archivos e IO

**Archivos clave:**
- `Models/IO/Importer.cs` — coordinador de importación
- `Models/IO/Exporter.cs` — coordinador de exportación
- `Models/Files/` — tipos de archivo soportados (cada uno hereda de `IoFileType`)

Tipos de archivo registrados: `.pixi`, `.png`, `.jpg`, `.bmp`, `.webp`, `.gif`, `.mp4`, `.svg`, `.ttf`, `.otf`.

Para agregar un tipo de archivo nuevo: crea una clase que herede de `IoFileType` en `Models/Files/` y regístrala en `ServiceCollectionHelpers.cs` como `AddSingleton<IoFileType, TuTipoDeArchivo>()`.

### Parsers de paletas de colores

**Archivos clave:**
- `Models/IO/PaletteParsers/` — un parser por formato de paleta

Formatos soportados: `.jasc` (PaintShop Pro), `.cls`, `.dpp` (Deluxe Paint), `.cpl` (CorelDraw), `.png` (paleta embebida), `.txt` (Paint.NET), `.hex`, `.gpl` (GIMP), `.pixi` (paleta nativa).

Para agregar un parser de paleta: crea una clase que herede de `PaletteFileParser` y regístrala en `ServiceCollectionHelpers.cs`.

### Preferencias y configuración

**Archivos clave:**
- `Models/Preferences/PreferencesSettings.cs` — sistema de preferencias
- `Models/Config/ConfigManager.cs` — configuraciones de la app
- `Views/Windows/Settings/SettingsWindow.axaml` — ventana de configuración

### Localización (i18n)

**Archivos clave:**
- `Data/Localization/Languages/` — archivos de idioma
- `Models/Localization/LocalizationProvider.cs` — proveedor de traducciones

### Renderizado en el viewport

**Archivos clave:**
- `Models/Rendering/` — lógica de renderizado de la escena
- `Views/Rendering/` — controles Avalonia que alojan la escena Drawie
- `Views/Main/ViewportControls/Viewport.axaml` — el canvas interactivo
