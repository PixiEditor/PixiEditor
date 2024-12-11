using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.Parser.Skia;

namespace PixiEditor.Models.Serialization;

public class SerializationConfig
{
    public ImageEncoder Encoder { get; set; }
    public ColorSpace ProcessingProcessingColorSpace { get; set; } = ColorSpace.CreateSrgbLinear();
    
    public SerializationConfig(ImageEncoder encoder, ColorSpace processingColorSpace)
    {
        Encoder = encoder;
        ProcessingProcessingColorSpace = processingColorSpace;
    }
}
