using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Parser;

namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

public class SurfaceSerializationFactory : SerializationFactory<ImageContainer, Surface>
{
    public SurfaceSerializationFactory(SerializationConfig config) : base(config)
    {
    }

    public override ImageContainer Serialize(Surface original)
    {
        var encoder = Config.Encoder;
        byte[] result = encoder.Encode(original.ToByteArray(), original.Size.X, original.Size.Y);
        ImageContainer container =
            new ImageContainer { ImageBytes = result, ResourceOffset = 0, ResourceSize = result.Length };
        
        return container;
    }

    public override Surface Deserialize(ImageContainer serialized)
    {
        throw new NotImplementedException();
    }
}
