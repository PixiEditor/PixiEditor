using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class VecISerializationFactory : SerializationFactory<byte[], VecI>
{
    public override string DeserializationId { get; } = "PixiEditor.VecI";

    public override byte[] Serialize(VecI original)
    {
        byte[] result = new byte[8];
        BitConverter.GetBytes(original.X).CopyTo(result, 0);
        BitConverter.GetBytes(original.Y).CopyTo(result, 4);
        
        return result;
    }

    public override bool TryDeserialize(object serialized, out VecI original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] { Length: 8 } bytes)
        {
            original = new VecI(BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4));
            return true;
        }

        original = default;
        return false; 
    }
}
