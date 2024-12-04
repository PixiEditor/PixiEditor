using MessagePack;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class ColorMatrixSerializationFactory : SerializationFactory<SerializableMatrix, ColorMatrix>
{
    public override SerializableMatrix Serialize(ColorMatrix original)
    {
        return new SerializableMatrix
        {
            Width = 4,
            Height = 5,
            Values = original.ToArray()
        };    
    }

    public override bool TryDeserialize(object raw, out ColorMatrix original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (raw is not Dictionary<string, object> serialized)
        {
            original = default;
            return false;
        }

        if (serialized.Count == 3)
        {
            float[] values = ExtractArray<float>(serialized["Values"]);
            original = new ColorMatrix(values);
            
            return true;
        }

        original = default;
        return false; 
    }

    public override string DeserializationId { get; } = "PixiEditor.Matrix";
}

[MessagePackObject]
class SerializableMatrix
{
    [Key("Width")]
    public int Width { get; set; }
    [Key("Height")]
    public int Height { get; set; }
    [Key("Values")]
    public float[] Values { get; set; }
}
