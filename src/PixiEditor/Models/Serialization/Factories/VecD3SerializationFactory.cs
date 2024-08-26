using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class VecD3SerializationFactory : SerializationFactory<byte[], VecD3>
{
    public override string DeserializationId { get; } = "PixiEditor.VecD";

    public override byte[] Serialize(VecD3 original)
    {
        byte[] result = new byte[sizeof(double) * 3];
        BitConverter.GetBytes(original.X).CopyTo(result, 0);
        BitConverter.GetBytes(original.Y).CopyTo(result, sizeof(double));
        BitConverter.GetBytes(original.Z).CopyTo(result, sizeof(double) * 2);
        return result;
    }

    public override bool TryDeserialize(object serialized, out VecD3 original)
    {
        if (serialized is byte[] { Length: sizeof(double) * 3 } bytes)
        {
            original = new VecD3(BitConverter.ToDouble(bytes, 0), BitConverter.ToDouble(bytes, sizeof(double)),
                BitConverter.ToDouble(bytes, sizeof(double) * 2));
            return true;
        }

        original = default;
        return false; 
    }
}
