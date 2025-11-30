using Drawie.Backend.Core;

namespace PixiEditor.Models.Serialization.Factories;

public class TextureSerializationFactory : SerializationFactory<byte[], Texture>
{
    private SurfaceSerializationFactory SurfaceFactory { get; } = new SurfaceSerializationFactory();

    public override byte[] Serialize(Texture original)
    {
        SurfaceFactory.Config = Config;
        SurfaceFactory.ResourceLocator = ResourceLocator;

        Surface surface = new Surface(original.Size);
        surface.DrawingSurface.Canvas.DrawSurface(original.DrawingSurface, 0, 0);
        return SurfaceFactory.Serialize(surface);
    }

    public override bool TryDeserialize(object serialized, out Texture original,
        (string serializerName, string serializerVersion) serializerData)
    {
        SurfaceFactory.Config = Config;
        SurfaceFactory.ResourceLocator = ResourceLocator;
        if (serialized is byte[] imgBytes)
        {
            if (SurfaceFactory.TryDeserialize(imgBytes, out Surface surface, serializerData))
            {
                original = new Texture(surface.Size);
                original.DrawingSurface.Canvas.DrawSurface(surface.DrawingSurface, 0, 0);
                return true;
            }
        }

        original = null;
        return false;
    }


    public override string DeserializationId { get; } = "PixiEditor.Texture";
}
