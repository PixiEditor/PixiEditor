using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

public class ColorSerializationFactory : SerializationFactory<byte[], Color>
{
    public override string DeserializationId { get; } = "PixiEditor.Color";
    public override byte[] Serialize(Color original)
    {
        byte[] result = new byte[4];
        result[0] = original.R;
        result[1] = original.G;
        result[2] = original.B;
        result[3] = original.A;
        return result; 
    }

    public override bool TryDeserialize(object serialized, out Color original)
    {
        if (serialized is byte[] { Length: 4 } bytes)
        {
            original = new Color(bytes[0], bytes[1], bytes[2], bytes[3]);
            return true;
        }

        original = default;
        return false; 
    }
}
