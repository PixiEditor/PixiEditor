# Documentación de YTB-Art (fork de PixiEditor)

Esta documentación describe la arquitectura completa del proyecto para que cualquier desarrollador pueda orientarse rápidamente al agregar funcionalidades nuevas o corregir errores.

## Índice de documentos

| # | Documento | Qué cubre |
|---|-----------|-----------|
| 01 | [Aplicación Principal](01-app-principal.md) | PixiEditor, Desktop y Browser — la app como tal |
| 02 | [Documento y Cambios](02-documento-y-cambios.md) | ChangeableDocument — modelo de datos con undo/redo |
| 03 | [Grafo de Nodos](03-grafo-de-nodos.md) | Sistema de nodos para composición no-destructiva |
| 04 | [Herramientas](04-herramientas.md) | Lápiz, selección, formas, texto y overlays |
| 05 | [Motor Drawie](05-motor-drawie.md) | Motor de renderizado 2D (Skia, OpenGL, Vulkan, WebGL) |
| 06 | [Imagen por Chunks](06-imagen-chunky.md) | ChunkyImageLib — imagen dividida en tiles |
| 07 | [Sistema de Extensiones](07-sistema-de-extensiones.md) | Plugins WebAssembly con SDK y runtime |
| 08 | [Formato .pixi y Serialización](08-formato-pixi-y-serializacion.md) | Parser de archivos, SVG, import/export |
| 09 | [UI Compartida](09-ui-compartida.md) | PixiDocks, ColorPicker, Zoombox, controles comunes |
| 10 | [Plataforma y Sistema Operativo](10-plataforma-y-sistema-operativo.md) | Steam, MSStore, Standalone, Windows, Linux, macOS |
| 11 | [Autenticación y Usuario](11-autenticacion-y-usuario.md) | PixiAuth, identidad, login |
| 12 | [Generadores y Utilidades](12-generadores-y-utilidades.md) | Source generators, FFmpeg, actualizaciones, cifrado |

---

## Flujo de arranque de la aplicación

```
PixiEditor.Desktop/Program.cs
    └── BuildAvaloniaApp() → StartWithClassicDesktopLifetime()
         └── App.axaml.cs → OnFrameworkInitializationCompleted()
              └── ClassicDesktopEntry(desktop)
                   ├── InitOperatingSystem()       → registra Windows/Linux/macOS
                   ├── InitApp(safeMode)
                   │    ├── InitPlatform()          → registra Steam/MSStore/Standalone
                   │    ├── ExtensionLoader.LoadExtensions()
                   │    └── ServiceCollection
                   │         .AddPlatform()
                   │         .AddPixiEditor(loader)  ← registro DI central
                   │         .AddExtensionServices(loader)
                   │         .BuildServiceProvider()
                   └── Load(viewModel, extensionLoader)
                        ├── ViewModelMain.Setup(services)
                        └── new MainWindow(loader) → Show()
```

## Dependencias principales entre subsistemas

```
┌─────────────────────────────────────────────────────────┐
│                    PixiEditor (UI + MVVM)                │
│  ViewModels ← Views ← Models ← Handlers                │
├───────────┬──────────┬─────────┬────────────────────────┤
│ Extensions│ Platform │ OS      │ UI.Common / ColorPicker │
│ Runtime   │ Steam    │ Windows │ PixiDocks / Zoombox     │
│ WASM      │ MSStore  │ Linux   │                         │
│ SDK       │ Standal. │ macOS   │                         │
├───────────┴──────────┴─────────┴────────────────────────┤
│         ChangeableDocument (Actions/Changes/CQRS)        │
│         DocumentChangeTracker ← NodeGraph ← Nodes        │
├──────────────────────────────────────────────────────────┤
│              ChunkyImageLib (imagen en tiles)             │
├──────────────────────────────────────────────────────────┤
│     Drawie (motor de renderizado)                        │
│     Backend.Core ← Backend.Skia | RenderApi.OpenGL/Vulk │
│     Numerics ← RenderApi ← Interop.Avalonia             │
├──────────────────────────────────────────────────────────┤
│  PixiParser (formato .pixi)  │  PixiEditor.SVG          │
└──────────────────────────────┴───────────────────────────┘
```

---

## Guía rápida: ¿Dónde trabajo para agregar...?

| Quiero agregar... | Documento | Archivos clave |
|---|---|---|
| Una herramienta nueva (ej: pincel especial) | [04-herramientas](04-herramientas.md) | `ViewModels/Tools/Tools/`, `ServiceCollectionHelpers.cs` |
| Un nodo nuevo al grafo | [03-grafo-de-nodos](03-grafo-de-nodos.md) | `ChangeableDocument/Changeables/Graph/Nodes/` |
| Un panel acoplable nuevo | [09-ui-compartida](09-ui-compartida.md) | `ViewModels/Dock/`, `Views/Dock/`, `LayoutManager.cs` |
| Soporte para un formato de archivo | [08-formato-pixi](08-formato-pixi-y-serializacion.md) | `Models/Files/`, `Models/IO/`, `ServiceCollectionHelpers.cs` |
| Un diálogo/popup nuevo | [01-app-principal](01-app-principal.md) | `Views/Dialogs/`, `Models/Dialogs/` |
| Un comando nuevo (menú o atajo) | [01-app-principal](01-app-principal.md) | Decorar método con `[Command.Basic]` en el SubViewModel |
| Una entrada de menú nueva | [01-app-principal](01-app-principal.md) | `ViewModels/Menu/MenuBuilders/` |
| Un filtro/efecto visual | [03-grafo-de-nodos](03-grafo-de-nodos.md) | `ChangeableDocument/Changeables/Graph/Nodes/FilterNodes/` |
| Una extensión/plugin | [07-extensiones](07-sistema-de-extensiones.md) | `samples/`, `Extensions.Sdk/` |
| Soporte para otra plataforma | [10-plataforma](10-plataforma-y-sistema-operativo.md) | `PixiEditor.Platform/`, `PixiEditor.OperatingSystem/` |
| Una operación nueva sobre el documento | [02-documento](02-documento-y-cambios.md) | `ChangeableDocument/Actions/`, `Changes/` |
| Un overlay sobre el canvas | [04-herramientas](04-herramientas.md) | `Views/Overlays/` |
| Preferencias/configuración nueva | [01-app-principal](01-app-principal.md) | `Models/Preferences/`, `Views/Windows/Settings/` |
| Localización/idioma nuevo | [09-ui-compartida](09-ui-compartida.md) | `Data/Localization/Languages/`, `UI.Common/Localization/` |

---

## Tecnologías principales

- **Framework UI:** Avalonia 11.x (cross-platform, similar a WPF)
- **Patrón:** MVVM con CommunityToolkit.Mvvm
- **DI:** Microsoft.Extensions.DependencyInjection
- **Renderizado:** Drawie (motor propio) sobre SkiaSharp 3.x + OpenGL/Vulkan
- **Serialización:** MessagePack (formato .pixi), Protobuf (extensiones)
- **Runtime de extensiones:** Wasmtime (WebAssembly)
- **Target:** .NET 8, multiplataforma (Windows, Linux, macOS, WebAssembly)
