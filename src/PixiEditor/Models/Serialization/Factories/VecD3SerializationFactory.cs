using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class VecD3SerializationFactory : SerializationFactory<byte[], Vec3D>
{
    public override string DeserializationId { get; } = "PixiEditor.VecD3";

    public override byte[] Serialize(Vec3D original)
    {
        byte[] result = new byte[sizeof(double) * 3];
        BitConverter.GetBytes(original.X).CopyTo(result, 0);
        BitConverter.GetBytes(original.Y).CopyTo(result, sizeof(double));
        BitConverter.GetBytes(original.Z).CopyTo(result, sizeof(double) * 2);
        return result;
    }

    public override bool TryDeserialize(object serialized, out Vec3D original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] { Length: sizeof(double) * 3 } bytes)
        {
            original = new Vec3D(BitConverter.ToDouble(bytes, 0), BitConverter.ToDouble(bytes, sizeof(double)),
                BitConverter.ToDouble(bytes, sizeof(double) * 2));
            return true;
        }

        original = default;
        return false; 
    }
}
