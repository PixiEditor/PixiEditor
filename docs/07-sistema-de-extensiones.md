# Sistema de Extensiones

## Proyectos incluidos

- `src/PixiEditor.Extensions.CommonApi/` — contratos públicos de la API (protobuf)
- `src/PixiEditor.Extensions/` — clase base `Extension` y servicios del host
- `src/PixiEditor.Extensions.Sdk/` — SDK para crear extensiones (compila a WASM)
- `src/PixiEditor.Extensions.WasmRuntime/` — runtime Wasmtime que ejecuta los módulos WASM
- `src/PixiEditor.Extensions.Runtime/` — cargador de extensiones desde disco
- `src/PixiEditor.Extensions.CommonApi.Diagnostics/` — analizador Roslyn para la API
- `src/PixiEditor.Extensions.Gen/` — generador de código para extensiones
- `src/PixiEditor.Extensions.MSBuild/` — tarea MSBuild para generar layouts
- `src/PixiEditor.Extensions.MSPackageBuilder/` — empaquetador de `.pixiext`
- `src/PixiEditor.Api.CGlueMSBuild/` — generador de C-glue para WASM
- `src/PixiEditor.WasmApi.Gen/` — generador de código WASM API
- `src/PixiEditor.WasmRuntime/` — runtime WASM genérico de bajo nivel
- `src/EncryptionLib/` — cifrado de paquetes de extensión

## Descripción

PixiEditor tiene un sistema de plugins basado en WebAssembly (WASM). Las extensiones se compilan a módulos WASM que corren dentro de una sandbox gestionada por Wasmtime. Esto significa que los plugins no tienen acceso directo al sistema ni al código del editor — solo pueden usar la API expuesta a través de contratos protobuf.

El flujo es:
1. Un desarrollador crea una extensión usando el SDK
2. La compila a WASM con las herramientas MSBuild incluidas
3. La empaqueta como un archivo `.pixiext`
4. PixiEditor la descubre, la carga con `ExtensionLoader` y la ejecuta en Wasmtime
5. La extensión se comunica con el editor a través de la CommonApi

Las extensiones pueden: registrar comandos, agregar herramientas, crear UI con FlyUI (un sistema declarativo), acceder a documentos, proveer paletas, y más.

## Propósito

Permitir que terceros extiendan PixiEditor de forma segura (sandbox WASM) sin acceso al código fuente. El sistema también se usa internamente para funcionalidades que se quieren distribuir como módulos opcionales.

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `Extensions.Runtime/ExtensionLoader.cs` | Descubre, valida y carga extensiones |
| `Extensions/Extension.cs` | Clase base que toda extensión debe implementar |
| `Extensions.CommonApi/` | Los contratos que definen qué puede hacer una extensión |
| `Extensions.WasmRuntime/WasmExtensionInstance.cs` | Instancia de una extensión WASM en ejecución |

---

## Grupos de funcionalidades

### Contratos de la API (CommonApi)

Lo que las extensiones "ven" del editor. Definido con interfaces y contratos protobuf.

**Carpeta:** `PixiEditor.Extensions.CommonApi/`

| Carpeta/Archivo | Qué permite a la extensión |
|---|---|
| `Commands/` | Registrar y ejecutar comandos del editor |
| `Tools/` | Registrar herramientas personalizadas |
| `Ui/` | Crear UI declarativa con FlyUI (layouts, botones, textos) |
| `Windowing/` | Abrir ventanas y popups |
| `Documents/` | Leer información del documento activo |
| `Palettes/` | Proveer paletas de colores |
| `IO/` | Acceso al sistema de archivos (sandboxeado) |
| `UserPreferences/` | Leer y escribir preferencias del usuario |
| `Logging/` | Escribir logs |
| `User/` | Información del usuario autenticado |

### Clase base de extensión

**Archivo:** `Extensions/Extension.cs`

Toda extensión hereda de `Extension` y tiene un ciclo de vida:

```csharp
public class MiExtension : Extension
{
    // Se llama cuando la extensión se carga
    protected override void OnLoaded() { }

    // Se llama cuando los servicios están listos
    protected override void OnInitialized() { }

    // Se llama cuando la ventana principal está visible
    protected override void OnMainWindowLoaded() { }

    // Se llama cuando el usuario ha completado el onboarding
    protected override void OnUserReady() { }
}
```

La extensión accede a servicios a través de `ExtensionServices`:

```csharp
// Registrar un comando
Api.Commands.RegisterCommand("mi.comando", "Mi Comando", MiAccion);

// Crear una ventana con FlyUI
var layout = new StackLayout(
    new Text("Hola desde mi extensión"),
    new Button("Clic aquí", OnClick)
);
Api.Windowing.CreatePopup("Mi Ventana", layout);
```

### FlyUI — Sistema de UI declarativa

**Carpeta:** `Extensions/FlyUI/`

FlyUI permite a las extensiones crear interfaces de usuario sin XAML, usando un sistema declarativo en código. Los elementos disponibles incluyen layouts (Stack, Grid), controles (Button, Text, TextBox, Slider, CheckBox), y contenedores (ScrollViewer, Border).

### Runtime y carga de extensiones

**Carpeta:** `Extensions.Runtime/`

- `ExtensionLoader.cs` — busca archivos `.pixiext` en las carpetas de extensiones, los descomprime y los registra
- `WasmExtensionEntry.cs` — envuelve una extensión WASM y gestiona su ciclo de vida
- `DllExtensionEntry.cs` — envuelve una extensión nativa (DLL) para desarrollo local

El loader busca en dos carpetas:
- La carpeta de extensiones del instalador (`InstallDirExtensionPackagesPath`)
- La carpeta local del usuario (`LocalExtensionPackagesPath`)

### Host WASM (WasmRuntime)

**Carpeta:** `Extensions.WasmRuntime/`

Implementa el lado "host" del puente WASM usando Wasmtime 22.0.0:
- `WasmExtensionInstance.cs` — una instancia de módulo WASM en ejecución
- `WasmRuntime.cs` — gestión del runtime
- `WasmMemoryUtility.cs` — serialización/deserialización de datos entre C# y WASM
- `Api/` — implementaciones de la API para WASM: `CommandApi`, `DocumentsApi`, `FlyUiApi`, `PreferencesApi`, `UserApi`, etc.

### SDK para crear extensiones

**Carpeta:** `Extensions.Sdk/`

El paquete NuGet que los autores de extensiones referencian. Compila el código C# a WebAssembly (WASI). Incluye:
- Targets MSBuild para compilar a WASM
- `build/` — tareas de empaquetado
- `native/` — código C de interop (glue)

### Herramientas de build

- `Extensions.MSBuild/` — tarea `GenerateLayoutFilesTask` que genera archivos de layout durante la compilación
- `Extensions.MSPackageBuilder/` — tarea `BuildPackageTask` que empaqueta la extensión en `.pixiext`
- `Api.CGlueMSBuild/` — genera el código C de interop para WASM + cifra recursos con AES

### Servicios de extensiones dentro de PixiEditor

El lado del editor que implementa los contratos de la CommonApi:

**Carpeta:** `PixiEditor/Models/ExtensionServices/`

- `WindowProvider.cs` — implementa `IWindowProvider` para las ventanas de extensiones
- `CommandProvider.cs`, `DocumentProvider.cs` — implementan los contratos de comandos y documentos
- `ToolsProvider.cs` — expone las herramientas a las extensiones
- `VisualTreeProvider.cs` — acceso al árbol visual de Avalonia
- `ConsoleLogger.cs` — implementación del logger

**En la UI:**
- `ViewModels/SubViewModels/ExtensionsViewModel.cs` — gestión de extensiones cargadas
- Los servicios se registran en `ServiceCollectionHelpers.AddExtensionServices()`

### Samples de extensiones

**Carpeta:** `samples/`

11 proyectos de ejemplo que demuestran cómo usar la API:
- `Sample1_HelloWorld` — extensión mínima
- `Sample2_Commands` — registrar comandos
- Hasta `Sample11_Brushes` — crear pinceles personalizados

Tienen su propia solución: `samples/PixiEditorExtensionSamples.sln`.
