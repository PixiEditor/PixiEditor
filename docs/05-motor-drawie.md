# Motor de Renderizado (Drawie)

## Proyectos incluidos

Este es un submodulo Git independiente en `src/Drawie/src/` con ~18 proyectos:

- **Drawie.Numerics** — tipos matemáticos (vectores, matrices, rectángulos)
- **Drawie.RenderApi** — abstracción de la API gráfica (GPU)
- **Drawie.RenderApi.OpenGl** — implementación OpenGL
- **Drawie.RenderApi.Vulkan** — implementación Vulkan
- **Drawie.RenderApi.WebGl** — implementación WebGL
- **Drawie.RenderApi.Web.Common** — código compartido para Web
- **Drawie.Backend.Core** — API de dibujo 2D abstracta (estilo Canvas/Skia)
- **Drawie.Backend.Skia** — implementación con SkiaSharp
- **Drawie.Windowing** — abstracción de ventanas
- **Drawie.Windowing.Glfw** — ventanas con GLFW
- **Drawie.Windowing.Browser** — ventanas para WebAssembly
- **Drawie.Interop.Avalonia** — integración con Avalonia (facade)
- **Drawie.Interop.Avalonia.Core** — base de la integración
- **Drawie.Interop.Avalonia.OpenGl** — integración OpenGL+Avalonia
- **Drawie.Interop.Avalonia.Vulkan** — integración Vulkan+Avalonia
- **Drawie.JSInterop** — interop con JavaScript (para browser)
- **DrawiEngine** — motor principal que une todo

## Descripción

Drawie es el motor de renderizado 2D del editor. Es un proyecto independiente (submodulo Git) que abstrae completamente la GPU y el backend de dibujo. El objetivo es que el código del editor pueda dibujar sin saber si está usando OpenGL, Vulkan, WebGL o SkiaSharp por debajo.

La arquitectura tiene capas bien definidas:
- **Capa matemática** (`Numerics`) — tipos básicos como vectores y matrices
- **Capa de GPU** (`RenderApi`) — abstracción de la API gráfica (texturas, contextos)
- **Capa de dibujo** (`Backend.Core`) — API de alto nivel tipo Canvas (dibujar líneas, formas, texto, imágenes)
- **Capa de integración** (`Interop.Avalonia`) — conecta el canvas de Drawie con un control visual de Avalonia

En escritorio, PixiEditor usa Drawie con Backend.Skia + RenderApi.OpenGl o Vulkan. En el navegador, usa Backend.Skia + RenderApi.WebGl.

## Propósito

Aislar PixiEditor de dependencias concretas de renderizado. Esto permite:
- Cambiar de backend gráfico sin tocar el código del editor
- Soportar múltiples plataformas (escritorio con OpenGL/Vulkan, browser con WebGL)
- Testear la lógica de dibujo sin una ventana real

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `DrawiEngine/DrawingEngine.cs` | Punto de entrada del motor: conecta RenderApi + Backend + Windowing |
| `Drawie.Backend.Core/Bridge/DrawingBackendApi.cs` | API global que el editor usa para acceder al backend |
| `Drawie.Interop.Avalonia/AppBuilderExtensions.cs` | Extensiones para agregar Drawie al pipeline de Avalonia |

---

## Grupos de funcionalidades

### Tipos matemáticos (Drawie.Numerics)

La base de todo: tipos numéricos que se usan en todo el editor.

**Carpeta:** `Drawie/src/Drawie.Numerics/`

| Tipo | Descripción |
|---|---|
| `VecI` | Vector 2D de enteros (posiciones de píxeles) |
| `VecD` | Vector 2D de doubles (posiciones precisas) |
| `VecF` | Vector 2D de floats |
| `Vec3D`, `Vec4D` | Vectores 3D y 4D |
| `RectI` | Rectángulo de enteros (bounds de capas) |
| `RectD` | Rectángulo de doubles |
| `Matrix3X3` | Matriz 3x3 para transformaciones 2D |
| `ColorMatrix`, `Matrix4x5F` | Matrices de color para filtros |
| `Kernel`, `KernelArray` | Kernels para convolución (blur, sharpen) |

Estos tipos son fundamentales — todo el editor los usa. Si necesitas un tipo geométrico, probablemente ya existe aquí.

### API de dibujo abstracta (Backend.Core)

Esta es la API de alto nivel que el editor usa para dibujar. Es similar a SkiaSharp o HTML Canvas, pero abstracta.

**Carpeta:** `Drawie/src/Drawie.Backend.Core/`

| Concepto | Clase/Carpeta | Descripción |
|---|---|---|
| Superficie | `Surfaces/DrawingSurface.cs` | Superficie donde se dibuja (equivale a SKSurface) |
| Canvas | `Surfaces/Canvas.cs` | Métodos de dibujo: líneas, rectángulos, texto, imágenes |
| Pintura | `PaintImpl/Paint.cs` | Estilo de pintura: color, opacidad, blend mode |
| Textura | `Texture.cs` | Imagen/bitmap en memoria |
| Texto | `Text/` | Fuentes, medición de texto, párrafos |
| Shaders | `Shaders/` | Shaders personalizados (SKSL) |
| Filtros | `ColorsImpl/` | Filtros de imagen, color matrix |
| Paths | `Vector/VectorPath.cs` | Paths vectoriales |
| Bridge | `Bridge/DrawingBackendApi.cs` | **Punto de acceso global al backend activo** |

El `DrawingBackendApi.Current` es el singleton que el editor usa para crear superficies, texturas, fuentes, etc. Es inyectado al inicio de la aplicación.

### Implementación Skia (Backend.Skia)

**Carpeta:** `Drawie/src/Drawie.Backend.Skia/`

Implementa toda la API de `Backend.Core` usando SkiaSharp 3.x. Es el backend predeterminado y el más usado. Cada clase abstracta de Core tiene su implementación concreta aquí: `SkiaSurface`, `SkiaCanvas`, `SkiaPaint`, etc.

### Abstracción de GPU (RenderApi)

Define cómo interactuar con la GPU a bajo nivel.

**Carpeta:** `Drawie/src/Drawie.RenderApi/`

Interfaces principales:
- `IRenderApi` — fábrica de APIs de renderizado por ventana
- `ITexture`, `IOpenGlTexture`, `IVkTexture` — texturas GPU
- `IOpenGlContext`, `IVulkanContext` — contextos de renderizado

Las implementaciones concretas:
- `Drawie.RenderApi.OpenGl/` — usa Silk.NET.OpenGL
- `Drawie.RenderApi.Vulkan/` — usa Silk.NET.Vulkan (incluye shaders SPIR-V)
- `Drawie.RenderApi.WebGl/` — para el navegador

### Integración con Avalonia (Interop)

Conecta el motor Drawie con la UI de Avalonia para que el canvas del editor sea un control visual nativo.

**Carpeta:** `Drawie/src/Drawie.Interop.Avalonia*/`

- `Drawie.Interop.Avalonia.Core/` — base: control Avalonia que aloja un canvas Drawie
- `Drawie.Interop.Avalonia.OpenGl/` — versión que usa superficie OpenGL
- `Drawie.Interop.Avalonia.Vulkan/` — versión que usa superficie Vulkan
- `Drawie.Interop.Avalonia/` — facade que agrupa ambas y se referencia desde PixiEditor

En el arranque, `AppBuilderExtensions` agrega Drawie al pipeline de Avalonia según el renderer disponible del SO.

### Windowing (standalone)

Para uso standalone de Drawie (sin Avalonia), hay un sistema de ventanas propio.

**Carpeta:** `Drawie/src/Drawie.Windowing/`

- `IWindow` — interfaz de ventana con eventos Update/Render
- `Drawie.Windowing.Glfw/` — ventanas de escritorio con GLFW
- `Drawie.Windowing.Browser/` — "ventana" en contexto de navegador

Esto se usa principalmente en los samples y demos de Drawie, no en PixiEditor directamente.

### Interop JavaScript (JSInterop)

**Carpeta:** `Drawie/src/Drawie.JSInterop/`

Puente entre C# y JavaScript para la versión WebAssembly. Permite llamar a APIs del navegador (WebGL, DOM) desde el código .NET.
