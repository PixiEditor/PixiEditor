using Drawie.Backend.Core.ColorsImpl.Paintables;
using Silk.NET.OpenGL;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

public class TexturePaintableSerializationFactory : SerializationFactory<byte[], TexturePaintable>,
    IPaintableSerializationFactory
{
    public override string DeserializationId { get; } = "PixiEditor.TexturePaintable";

    private readonly TextureSerializationFactory textureFactory = new TextureSerializationFactory();

    public override byte[] Serialize(TexturePaintable original)
    {
        ByteBuilder builder = new();
        Serialize(original, builder);

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out TexturePaintable original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] bytes)
        {
            original = null!;
            return false;
        }

        ByteExtractor extractor = new(bytes);
        original = TryDeserialize(extractor) as TexturePaintable;

        return true;
    }

    public Paintable TryDeserialize(ByteExtractor extractor)
    {
        return TryDeserialize(extractor, default);
    }

    public Paintable TryDeserialize(ByteExtractor extractor,
        (string serializerName, string serializerVersion) serializerData)
    {
        textureFactory.Config = Config;
        textureFactory.ResourceLocator = ResourceLocator;

        int length = extractor.GetInt();
        var textureData = extractor.GetByteSpan(length).ToArray();
        if (textureFactory.TryDeserialize(textureData, out var tex, serializerData))
        {
            return new TexturePaintable(tex);
        }

        return null!;
    }

    public void Serialize(Paintable paintable, ByteBuilder builder)
    {
        if (paintable is not TexturePaintable texturePaintable)
        {
            throw new ArgumentException("Paintable is not a TexturePaintable", nameof(paintable));
        }

        textureFactory.Config = Config;
        textureFactory.ResourceLocator = ResourceLocator;

        var array = textureFactory.Serialize(texturePaintable.Image);
        builder.AddInt(array.Length);
        builder.AddByteArray(array);
    }
}
