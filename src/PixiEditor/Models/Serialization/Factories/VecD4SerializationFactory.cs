using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class VecD4SerializationFactory : SerializationFactory<byte[], Vec4D>
{
    public override string DeserializationId { get; } = "PixiEditor.VecD4";

    public override byte[] Serialize(Vec4D original)
    {
        byte[] result = new byte[sizeof(double) * 4];
        BitConverter.GetBytes(original.X).CopyTo(result, 0);
        BitConverter.GetBytes(original.Y).CopyTo(result, sizeof(double));
        BitConverter.GetBytes(original.Z).CopyTo(result, sizeof(double) * 2);
        BitConverter.GetBytes(original.W).CopyTo(result, sizeof(double) * 3);
        return result;
    }

    public override bool TryDeserialize(object serialized, out Vec4D original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] { Length: sizeof(double) * 4 } bytes)
        {
            original = new Vec4D(
                BitConverter.ToDouble(bytes, 0),
                BitConverter.ToDouble(bytes, sizeof(double)),
                BitConverter.ToDouble(bytes, sizeof(double) * 2),
                BitConverter.ToDouble(bytes, sizeof(double) * 3));
            return true;
        }

        original = default;
        return false; 
    }
}
