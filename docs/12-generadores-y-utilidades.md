# Generadores de Código, Renderizado de Animaciones y Utilidades

## Proyectos incluidos

**Generadores de código (Roslyn Source Generators):**
- `src/PixiEditor.Gen/` — genera lista de nombres de comandos y evaluadores
- `src/PixiEditor.ChangeableDocument.Gen/` — genera clases de Action/Change
- `src/PixiEditor.Extensions.Gen/` — genera bindings para extensiones
- `src/PixiEditor.WasmApi.Gen/` — genera código WASM API

**Renderizado de animaciones:**
- `src/PixiEditor.AnimationRenderer.Core/` — interfaz de renderizado de animaciones
- `src/PixiEditor.AnimationRenderer.FFmpeg/` — implementación con FFmpeg

**Sistema de actualizaciones:**
- `src/PixiEditor.UpdateModule/` — verificación y descarga de actualizaciones
- `src/PixiEditor.UpdateInstaller/` — biblioteca del instalador
- `src/PixiEditor.UpdateInstaller.Exe/` — ejecutable del instalador

**Cifrado:**
- `src/EncryptionLib/` — cifrado AES para paquetes de extensión
- `src/ResourceEncryptor/` — herramienta CLI para cifrar recursos

**Utilidades comunes:**
- `src/PixiEditor.Common/` — tipos compartidos sin dependencias

## Descripción

Este grupo reúne los proyectos de infraestructura que no pertenecen a ningún dominio específico del editor. Son herramientas que se usan durante la compilación (generadores de código), durante la exportación (FFmpeg), durante la distribución (actualizaciones, cifrado) o como base para otros proyectos (utilidades comunes).

## Propósito

Reducir código repetitivo con generadores automáticos, habilitar la exportación de animaciones a video, gestionar las actualizaciones automáticas, y proveer utilidades compartidas mínimas.

---

## Grupos de funcionalidades

### Generadores de código Roslyn

Los source generators son plugins del compilador de C# que generan código fuente automáticamente durante la compilación. Eliminan boilerplate y reducen errores.

#### PixiEditor.Gen

**Carpeta:** `src/PixiEditor.Gen/`

Genera una lista estática de todos los nombres de comandos y evaluadores a partir de los atributos `[Command.Basic]` y `[Evaluator.CanExecute]` encontrados en el código. Esto evita usar strings hardcodeados para referenciar comandos.

#### ChangeableDocument.Gen

**Carpeta:** `src/PixiEditor.ChangeableDocument.Gen/`

Lee los constructores decorados con `[GenerateMakeChangeAction]` y `[GenerateUpdateableChangeAction]` en los archivos de Changes, y genera automáticamente las clases Action correspondientes. Esto es crucial: sin este generador, por cada Change habría que escribir manualmente una Action casi idéntica.

#### Extensions.Gen

**Carpeta:** `src/PixiEditor.Extensions.Gen/`

Genera código de binding para la API de extensiones, facilitando el hot-reload de extensiones durante el desarrollo.

#### WasmApi.Gen

**Carpeta:** `src/PixiEditor.WasmApi.Gen/`

Genera el "glue code" que conecta las llamadas C# del SDK de extensiones con las importaciones WASM. Reescribe cuerpos de métodos para que las llamadas a la API se conviertan en invocaciones WASM.

**Nota general:** si necesitas modificar lo que un generador produce, busca la clase que implementa `ISourceGenerator` o `IIncrementalGenerator` dentro de la carpeta del generador. Los generadores son proyectos `netstandard2.0` (requisito de Roslyn).

### Renderizado de animaciones a video

El editor puede exportar animaciones como video (MP4) usando FFmpeg como proceso externo.

**Carpeta:** `src/PixiEditor.AnimationRenderer.Core/`

- `IAnimationRenderer.cs` — interfaz: recibe una lista de frames y produce un archivo de video
- `Frame.cs` — un frame individual (imagen + timestamp)
- `QualityPreset.cs` — presets de calidad (baja, media, alta)

**Carpeta:** `src/PixiEditor.AnimationRenderer.FFmpeg/`

- `FFMpegRenderer.cs` — implementación que lanza FFmpeg como proceso y le envía los frames por stdin (pipe)
- Incluye binarios de FFmpeg para Windows, Linux y macOS según `RuntimeIdentifier`
- Usa la biblioteca `FFMpegCore 5.1.0` como wrapper

En el DI se registra así:

```csharp
.AddSingleton<IDocumentBuilder, AnimationDocumentBuilder>(x =>
    new AnimationDocumentBuilder(new FFMpegRenderer()))
```

### Sistema de actualizaciones

Gestiona la verificación, descarga e instalación de actualizaciones de la aplicación.

**Carpeta:** `src/PixiEditor.UpdateModule/`

- `UpdateChecker.cs` — consulta la API de releases para verificar si hay versiones nuevas
- `UpdateDownloader.cs` — descarga el paquete de actualización
- `UpdateInstaller.cs` — aplica la actualización
- `ReleaseInfo.cs` — información de la release (versión, URL, notas)
- `UpdateChannel.cs` — canales: Stable y Nightly

**Carpeta:** `src/PixiEditor.UpdateInstaller/`

Biblioteca del instalador de actualizaciones. Se usa desde el ejecutable separado.

**Carpeta:** `src/PixiEditor.UpdateInstaller.Exe/`

Ejecutable standalone (WinExe, single-file) que realiza la instalación de la actualización. Se lanza como proceso separado cuando la app necesita actualizarse. Incluye integración con InnoSetup para Windows.

El flujo completo:
1. `UpdateChecker` detecta una versión nueva
2. `UpdateSubViewModel` muestra la notificación al usuario
3. Si acepta, `UpdateDownloader` descarga el paquete
4. Se lanza `UpdateInstaller.Exe` como proceso separado
5. El instalador reemplaza los archivos y reinicia la app

### Cifrado de recursos

**Carpeta:** `src/EncryptionLib/`

Biblioteca de cifrado simétrico (AES-128) usada para proteger los recursos de las extensiones empaquetadas. Los archivos `.pixiext` contienen recursos cifrados que solo el host puede descifrar.

- `PackageEncryptor.EncryptResources()` — cifra archivos con una clave AES
- `PackageEncryptor.DecryptResources()` — descifra archivos

**Carpeta:** `src/ResourceEncryptor/`

Herramienta de línea de comandos (compilada con AOT para máximo rendimiento) que usa `EncryptionLib` para cifrar los recursos durante el proceso de build de extensiones. Se invoca desde las tareas MSBuild.

### Utilidades comunes (PixiEditor.Common)

**Carpeta:** `src/PixiEditor.Common/`

Biblioteca mínima sin dependencias externas, compartida por múltiples proyectos:

- `MathEx.cs` — funciones matemáticas que no están en `System.Math` (interpolación, clamp, etc.)
- `ICacheable.cs` — interfaz para objetos con caché invalidable (define un hash que cambia cuando el objeto se modifica)

Es intencionalmente pequeña: solo contiene lo que necesitan múltiples proyectos sin crear dependencias circulares.

### Scripts de build

**Carpeta:** `src/PixiEditor.AvaloniaUI/`

Contiene scripts de CI/CD (`build.ps1`, `build.sh`) para compilar y empaquetar la aplicación. No es un proyecto .csproj.

**Carpeta:** `src/PixiEditor.Builder/`

Scripts adicionales de build. Tampoco es un proyecto .csproj.

### Tests

Los proyectos de testing están en `tests/` con su propia solución `tests/PixiEditorTests.sln`:

| Proyecto | Qué testea |
|---|---|
| `PixiEditor.Tests` | Tests principales del editor |
| `PixiEditor.Backend.Tests` | Tests del backend de dibujo |
| `PixiEditor.Extensions.Tests` | Tests del sistema de extensiones |
| `PixiEditor.Extensions.Sdk.Tests` | Tests del SDK de extensiones |
| `PixiEditor.Extensions.WasmRuntime.Tests` | Tests del runtime WASM |
| `PixiEditor.Api.CGlueMSBuild.Tests` | Tests del C-glue |
| `ChunkyImageLibTest` | Tests de ChunkyImageLib |

Además, los submodulos tienen sus propios tests:
- `Drawie/src/Drawie.Tests/`
- `PixiParser/src/PixiParser.Tests/`
- `PixiDocks/PixiDock.Tests/`
