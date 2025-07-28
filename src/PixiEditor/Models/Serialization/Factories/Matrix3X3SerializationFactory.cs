using Drawie.Backend.Core.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class Matrix3X3SerializationFactory : SerializationFactory<byte[], Matrix3X3>
{
    public override string DeserializationId { get; } = "PixiEditor.Matrix3X3";

    public override byte[] Serialize(Matrix3X3 original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddMatrix3X3(original);

        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out Matrix3X3 original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes)
        {
            ByteExtractor extractor = new ByteExtractor(bytes);
            original = extractor.GetMatrix3X3();
            return true;
        }

        original = default;
        return false;
    }
}
