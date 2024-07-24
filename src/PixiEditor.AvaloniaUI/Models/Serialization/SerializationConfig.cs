using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Models.Serialization;

public class SerializationConfig
{
    public ImageEncoder Encoder { get; set; }
    
    public SerializationConfig(ImageEncoder encoder)
    {
        Encoder = encoder;
    }
}
