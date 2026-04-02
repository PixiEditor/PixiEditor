# Formato .pixi y Serialización

## Proyectos incluidos

- `src/PixiParser/src/PixiParser/` — parser del formato `.pixi` (MessagePack)
- `src/PixiParser/src/PixiParser.Skia/` — extensión para codificar/decodificar imágenes con Skia
- `src/PixiEditor.SVG/` — parser y modelo SVG propio
- `src/PixiEditor/Models/IO/` — coordinadores de importación y exportación
- `src/PixiEditor/Models/Serialization/` — factories de serialización del grafo
- `src/PixiEditor/Models/Files/` — tipos de archivo soportados

## Descripción

PixiEditor usa un formato de archivo propio llamado `.pixi` para guardar documentos con toda su información (capas, grafo de nodos, animaciones, paletas, etc.). Internamente usa MessagePack como formato de serialización binaria.

Además del formato nativo, el editor soporta importar y exportar múltiples formatos de imagen (PNG, JPEG, BMP, WebP, GIF), video (MP4 vía FFmpeg), SVG, y fuentes (TTF/OTF). Cada formato tiene su propio handler.

El sistema de serialización del grafo de nodos usa "factories" — una por tipo de dato — que saben cómo convertir cada propiedad del grafo a un formato serializable y viceversa.

## Propósito

- Persistir el estado completo del documento de forma que se pueda restaurar exactamente
- Soportar múltiples formatos de entrada y salida
- Mantener compatibilidad hacia atrás con versiones anteriores del formato `.pixi`

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `PixiParser/src/PixiParser/PixiParser.cs` | Lee y escribe archivos `.pixi` |
| `PixiEditor/Models/IO/Importer.cs` | Coordinador de importación de archivos |
| `PixiEditor/Models/IO/Exporter.cs` | Coordinador de exportación de archivos |
| `PixiEditor/Models/Files/` | Tipos de archivo registrados |
| `PixiEditor.SVG/SvgDocument.cs` | Modelo del documento SVG |

---

## Grupos de funcionalidades

### Parser del formato .pixi

**Carpeta:** `PixiParser/src/PixiParser/`

El formato `.pixi` es binario, basado en MessagePack 2.5. El parser soporta múltiples versiones del formato para mantener compatibilidad hacia atrás.

**Archivos clave:**
- `PixiParser.cs` — clase principal con métodos `Serialize()` y `Deserialize()` (síncronos y asíncronos)
- `IPixiParser` — interfaz para el parser
- `IPixiDocument` — interfaz del documento deserializado
- `Document.cs` — modelo del documento con todas sus propiedades
- `Graph/` — representación del grafo de nodos en el formato de archivo
- `KeyFrameData.cs` — datos de animación
- `ReferenceLayer.cs` — capa de referencia
- `ResourceStorage.cs` — recursos embebidos (imágenes, fuentes)

**Extension Skia** en `PixiParser/src/PixiParser.Skia/`:
- Agrega soporte para codificar/decodificar imágenes de capas usando SkiaSharp + formato QOI (Quick OK Image)

Si necesitas agregar un campo nuevo al formato `.pixi`, debes actualizar el schema en el parser y asegurarte de que las versiones anteriores sigan siendo legibles.

### Serialización del grafo de nodos

Cuando el editor guarda un archivo `.pixi`, necesita serializar todo el grafo de nodos con sus propiedades. Cada tipo de dato tiene una "factory" que sabe cómo convertirlo.

**Carpeta:** `PixiEditor/Models/Serialization/Factories/`

Factories existentes (cada una maneja un tipo):

| Factory | Tipo que serializa |
|---|---|
| `ColorSerializationFactory` | Colores |
| `SurfaceSerializationFactory` | Superficies/texturas de imagen |
| `ChunkyImageSerializationFactory` | Imágenes en chunks |
| `VectorPathSerializationFactory` | Paths vectoriales |
| `TextSerializationFactory` | Datos de texto |
| `VecDSerializationFactory`, `VecISerializationFactory` | Vectores |
| `BrushSerializationFactory` | Pinceles |
| `FontFamilySerializationFactory` | Fuentes |
| `KernelSerializationFactory` | Kernels de convolución |
| `ColorMatrixSerializationFactory` | Matrices de color |
| `EllipseSerializationFactory`, `RectangleSerializationFactory`, `LineSerializationFactory` | Formas geométricas |
| `Paintables/*SerializationFactory` | Gradientes, colores de pintura, texturas |

Para agregar soporte de serialización para un tipo nuevo: crea una factory en esta carpeta y regístrala en `ServiceCollectionHelpers.AddSerializationFactories()`.

### Tipos de archivo soportados

Cada formato tiene su clase que define extensiones, capacidades de lectura/escritura y el encoder correspondiente.

**Carpeta:** `PixiEditor/Models/Files/`

| Clase | Formato | Lee | Escribe |
|---|---|---|---|
| `PixiFileType` | `.pixi` | Sí | Sí |
| `PngFileType` | `.png` | Sí | Sí |
| `JpegFileType` | `.jpg`, `.jpeg` | Sí | Sí |
| `BmpFileType` | `.bmp` | Sí | Sí |
| `WebpFileType` | `.webp` | Sí | Sí |
| `GifFileType` | `.gif` | Sí | Sí |
| `Mp4FileType` | `.mp4` | Sí | Sí |
| `SvgFileType` | `.svg` | Sí | Sí |
| `TtfFileType` | `.ttf` | Sí | No |
| `OtfFileType` | `.otf` | Sí | No |

Para agregar un formato nuevo: crea una clase que herede de `IoFileType` y regístrala en `ServiceCollectionHelpers.cs`:

```csharp
.AddSingleton<IoFileType, MiFormatoFileType>()
```

### Importación y exportación

**Carpeta:** `PixiEditor/Models/IO/`

- `Importer.cs` — detecta el tipo de archivo por extensión y delega al handler correcto
- `Exporter.cs` — genera el archivo de salida según el formato elegido
- `FileEncoders/` — encoders específicos por formato de imagen

### Constructores de documentos especiales

**Carpeta:** `PixiEditor/Models/IO/CustomDocumentFormats/`

Algunos formatos no son imágenes simples — requieren lógica especial para convertirlos en un documento del editor:

- `SvgDocumentBuilder` — importa un SVG y crea capas vectoriales
- `FontDocumentBuilder` — importa una fuente TTF/OTF y crea un documento con los glifos
- `AnimationDocumentBuilder` — importa un video MP4 y crea frames de animación (usa FFmpeg)

Cada builder implementa `IDocumentBuilder` y se registra en DI.

### Parser SVG propio

**Carpeta:** `src/PixiEditor.SVG/`

PixiEditor tiene su propio parser SVG (no usa librerías externas). Incluye:

- `SvgDocument.cs` — representación del árbol SVG completo
- `SvgParser.cs` — parseo de texto SVG al modelo de objetos
- `Elements/` — todos los elementos SVG: `SvgPath`, `SvgCircle`, `SvgRect`, `SvgGroup`, `SvgFilter`, gradientes, etc.
- `Features/` — interfaces para capacidades: `IFillable`, `IStrokable`, `ITransformable`, `IFilterable`
- `Units/` — unidades SVG tipadas (px, em, %, etc.)

### Parsers de paletas de colores

**Carpeta:** `PixiEditor/Models/IO/PaletteParsers/`

Formatos de paleta soportados:

| Parser | Formato |
|---|---|
| `JascFileParser` | `.jasc` (PaintShop Pro) |
| `ClsFileParser` | `.cls` |
| `DeluxePaintParser` | `.dpp` (Deluxe Paint) |
| `CorelDrawPalParser` | `.cpl` (CorelDraw) |
| `PngPaletteParser` | `.png` (paleta embebida) |
| `PaintNetTxtParser` | `.txt` (Paint.NET) |
| `HexPaletteParser` | `.hex` |
| `GimpGplParser` | `.gpl` (GIMP) |
| `PixiPaletteParser` | `.pixi` (paleta nativa) |

Para agregar un parser de paleta: crea una clase que herede de `PaletteFileParser` y regístrala en DI.
