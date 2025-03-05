using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

public class ColorPaintableSerializationFactory : SerializationFactory<byte[], ColorPaintable>,
    IPaintableSerializationFactory
{
    public override string DeserializationId { get; } = "PixiEditor.ColorPaintable";

    public override byte[] Serialize(ColorPaintable original)
    {
        ByteBuilder builder = new();
        Serialize(original, builder);

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out ColorPaintable original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] bytes)
        {
            original = null!;
            return false;
        }

        ByteExtractor extractor = new(bytes);
        original = TryDeserialize(extractor) as ColorPaintable;

        return true;
    }

    public Paintable? TryDeserialize(ByteExtractor extractor)
    {
        Color color = extractor.GetColor();
        return new ColorPaintable(color);
    }

    public void Serialize(Paintable paintable, ByteBuilder builder)
    {
        builder.AddColor((paintable as ColorPaintable).Color);
    }
}
