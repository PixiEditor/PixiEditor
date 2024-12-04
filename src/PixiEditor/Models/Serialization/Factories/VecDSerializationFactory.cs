using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class VecDSerializationFactory : SerializationFactory<byte[], VecD>
{
    public override string DeserializationId { get; } = "PixiEditor.VecD";

    public override byte[] Serialize(VecD original)
    {
        byte[] result = new byte[sizeof(double) * 2];
        BitConverter.GetBytes(original.X).CopyTo(result, 0);
        BitConverter.GetBytes(original.Y).CopyTo(result, sizeof(double));
        return result;
    }

    public override bool TryDeserialize(object serialized, out VecD original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] { Length: sizeof(double) * 2 } bytes)
        {
            original = new VecD(BitConverter.ToDouble(bytes, 0), BitConverter.ToDouble(bytes, sizeof(double)));
            return true;
        }

        original = default;
        return false; 
    }
}
