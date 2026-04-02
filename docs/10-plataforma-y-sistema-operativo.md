# Abstracción de Plataforma y Sistema Operativo

## Proyectos incluidos

**Plataforma de distribución:**
- `src/PixiEditor.Platform/` — interfaz `IPlatform`
- `src/PixiEditor.Platform.Standalone/` — distribución directa (GitHub, instalador)
- `src/PixiEditor.Platform.Steam/` — distribución por Steam
- `src/PixiEditor.Platform.MSStore/` — distribución por Microsoft Store

**Sistema operativo:**
- `src/PixiEditor.OperatingSystem/` — interfaz `IOperatingSystem`
- `src/PixiEditor.Windows/` — implementación Windows
- `src/PixiEditor.Linux/` — implementación Linux
- `src/PixiEditor.MacOs/` — implementación macOS

## Descripción

El editor necesita adaptarse a dos dimensiones independientes:

1. **La plataforma de distribución**: ¿se vende en Steam, en la Microsoft Store, o se distribuye directamente? Cada tienda tiene sus propios mecanismos de autenticación, DLC, actualizaciones y licencias.

2. **El sistema operativo**: ¿es Windows, Linux o macOS? Cada SO tiene sus propias APIs para criptografía, gestión de procesos, teclas del teclado, y apertura de archivos/URLs.

Ambas dimensiones se resuelven con interfaces abstractas que se inyectan al inicio de la aplicación según los **símbolos de compilación** (`STEAM`, `MSIX`, `WINDOWS`, `LINUX`, `MACOS`). El editor nunca llama directamente a APIs nativas — siempre pasa por estas abstracciones.

## Propósito

Permitir que el mismo código base funcione en múltiples combinaciones de SO + tienda sin cambios. El código del editor usa `IPlatform.Current` e `IOperatingSystem.Current` sin saber qué implementación concreta hay detrás.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `PixiEditor.Platform/IPlatform.cs` | Contrato de la plataforma de distribución |
| `PixiEditor.OperatingSystem/IOperatingSystem.cs` | Contrato del sistema operativo |
| `PixiEditor/Initialization/ClassicDesktopEntry.cs` | Donde se decide qué implementación usar |

---

## Grupos de funcionalidades

### Plataforma de distribución (IPlatform)

**Archivo:** `PixiEditor.Platform/IPlatform.cs`

Define qué capacidades ofrece la tienda:

- `Id` y `Name` — identificador y nombre de la plataforma
- `PerformHandshake()` — inicialización con la tienda (ej: iniciar Steam SDK)
- `AdditionalContentProvider` — proveedor de contenido adicional (DLC, packs de pinceles)
- `IdentityProvider` — proveedor de identidad (login del usuario)
- `HasSelfContainedUpdater` — si la plataforma maneja sus propias actualizaciones

**Cómo se selecciona la plataforma:**

En `ClassicDesktopEntry.GetActivePlatform()`, según los símbolos de compilación:

```csharp
#if STEAM || DEV_STEAM
    return new SteamPlatform();
#elif MSIX || MSIX_DEBUG
    return new MicrosoftStorePlatform(...);
#else
    return new StandalonePlatform(...);
#endif
```

**Implementaciones:**

| Proyecto | Plataforma | Dependencias especiales |
|---|---|---|
| `Platform.Standalone` | Distribución directa | PixiAuth para identidad |
| `Platform.Steam` | Steam | Steamworks.NET (DLLs nativas de Steam incluidas) |
| `Platform.MSStore` | Microsoft Store | PixiAuth para identidad |

La plataforma Steam incluye las DLLs nativas de `steam_api64` para todas las combinaciones de OS+arquitectura: win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, osx-arm64.

### Sistema operativo (IOperatingSystem)

**Archivo:** `PixiEditor.OperatingSystem/IOperatingSystem.cs`

Define qué capacidades ofrece el SO:

- `Name` — nombre del SO
- `AnalyticsId` — ID anónimo del dispositivo (para analytics)
- `InputKeys` — abstracción de las teclas del teclado físico
- `ProcessUtility` — lanzar y gestionar procesos externos
- `Encryptor` — cifrado de datos en almacenamiento local
- `OpenUri()`, `OpenFolder()` — abrir URLs y carpetas
- `GetAvailableRenderers()` — listar renderers GPU disponibles (OpenGL, Vulkan)
- `HandleNewInstance()` — manejar cuando se intenta abrir una segunda instancia de la app

**Cómo se selecciona el OS:**

En `ClassicDesktopEntry.GetActiveOperatingSystem()`:

```csharp
#if WINDOWS
    return new WindowsOperatingSystem();
#elif LINUX
    return new LinuxOperatingSystem();
#elif MACOS
    return new MacOperatingSystem();
#endif
```

**Implementaciones:**

| Proyecto | OS | Dependencias especiales |
|---|---|---|
| `PixiEditor.Windows` | Windows | Avalonia.Win32, System.Security.Cryptography.ProtectedData (DPAPI) |
| `PixiEditor.Linux` | Linux | DeviceId.Linux |
| `PixiEditor.MacOs` | macOS | DeviceId.Mac |

### Registro en DI

Ambas abstracciones se registran como singletons estáticos accesibles globalmente:

```csharp
// Se registra una vez al inicio
IOperatingSystem.RegisterOS(new WindowsOperatingSystem());
IPlatform.RegisterPlatform(new StandalonePlatform(...));

// Se accede desde cualquier parte
IOperatingSystem.Current.OpenUri("https://...");
IPlatform.Current.AdditionalContentProvider.GetContent();
```

### Configuraciones de compilación

El archivo `src/Directory.Build.props` define qué símbolos se activan para cada configuración:

| Configuración | Símbolos | Plataforma + OS |
|---|---|---|
| `Debug` | `WINDOWS` (o `LINUX`/`MACOS` según OS) | Standalone + OS detectado |
| `Release` | `WINDOWS` + `UPDATE` + `RELEASE` | Standalone con actualizaciones |
| `Steam` | `STEAM` + `WINDOWS` | Steam + Windows |
| `MSIX` | `MSIX` + `WINDOWS` | Microsoft Store + Windows |
| `DevRelease` | `RELEASE` + `WINDOWS` | Release para desarrollo |
| `DevSteam` | `DEV_STEAM` + `WINDOWS` | Steam para desarrollo |

### Cómo agregar soporte para una plataforma nueva

1. Crea un proyecto `PixiEditor.Platform.MiPlataforma/` que implemente `IPlatform`.
2. Agrega el símbolo de compilación en `Directory.Build.props`.
3. Agrega el `#elif` correspondiente en `ClassicDesktopEntry.GetActivePlatform()`.
4. Registra las dependencias en `PlatformServiceCollection.AddPlatform()`.

### Cómo agregar soporte para un OS nuevo

1. Crea un proyecto `PixiEditor.MiOS/` que implemente `IOperatingSystem`.
2. Agrega el símbolo de compilación en `Directory.Build.props`.
3. Agrega el `#elif` correspondiente en `ClassicDesktopEntry.GetActiveOperatingSystem()`.
