using PixiEditor.Helpers;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Skia;
using PixiEditor.Parser.Skia;

namespace PixiEditor.Models.Serialization.Factories;

public class SurfaceSerializationFactory : SerializationFactory<byte[], Surface>
{
    public override byte[] Serialize(Surface original)
    {
        var encoder = Config.Encoder;
        byte[] result = encoder.Encode(original.ToByteArray(), original.Size.X, original.Size.Y);

        return result;
    }

    public override bool TryDeserialize(object serialized, out Surface original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] imgBytes)
        {
            original = DecodeSurface(imgBytes, Config.Encoder);
            return true;
        }
        
        original = null;
        return false;
    }


    public static Surface DecodeSurface(byte[] imgBytes, ImageEncoder encoder)
    {
        byte[] decoded =
            encoder.Decode(imgBytes, out SKImageInfo info);
        using Image img = Image.FromPixels(info.ToImageInfo(), decoded);
        Surface surface = new Surface(img.Size);
        surface.DrawingSurface.Canvas.DrawImage(img, 0, 0);

        return surface;
    }


    public override string DeserializationId { get; } = "PixiEditor.Surface";
}
