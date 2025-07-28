using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

internal abstract class GradientPaintableSerializationFactory<T> : SerializationFactory<byte[], T>,
    IPaintableSerializationFactory
    where T : GradientPaintable
{
    public override byte[] Serialize(T original)
    {
        ByteBuilder builder = new();
        Serialize(original, builder);

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out T original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] bytes)
        {
            original = null!;
            return false;
        }

        ByteExtractor extractor = new(bytes);
        original = TryDeserialize(extractor) as T;

        return true;
    }

    public Paintable TryDeserialize(ByteExtractor extractor) => TryDeserializeGradient(extractor);
    public void Serialize(Paintable paintable, ByteBuilder builder) => SerializeGradient((T)paintable, builder);

    protected void SerializeGradient(T paintable, ByteBuilder builder)
    {
        builder.AddBool(paintable.AbsoluteValues);
        bool hasTransform = paintable.Transform.HasValue && paintable.Transform.Value != Matrix3X3.Identity;
        builder.AddBool(hasTransform);
        if (hasTransform)
        {
            builder.AddMatrix3X3(paintable.Transform.Value);
        }

        builder.AddInt(paintable.GradientStops?.Count ?? 0);
        foreach (var stop in paintable.GradientStops)
        {
            builder.AddColor(stop.Color);
            builder.AddDouble(stop.Offset);
        }

        SerializeSpecificGradient(paintable, builder);
    }

    protected T TryDeserializeGradient(ByteExtractor extractor)
    {
        bool absoluteValues = extractor.GetBool();
        Matrix3X3? transform = null;
        if (extractor.GetBool())
        {
            transform = extractor.GetMatrix3X3();
        }

        int stopsCount = extractor.GetInt();
        List<GradientStop> stops = new();
        for (int i = 0; i < stopsCount; i++)
        {
            stops.Add(new GradientStop(extractor.GetColor(), extractor.GetDouble()));
        }

        T paintable = DeserializeGradient(absoluteValues, transform, stops, extractor);
        return paintable;
    }

    protected abstract void SerializeSpecificGradient(T paintable, ByteBuilder builder);
    protected abstract T DeserializeGradient(bool absoluteValues, Matrix3X3? transform, List<GradientStop> stops,
        ByteExtractor extractor);
}
