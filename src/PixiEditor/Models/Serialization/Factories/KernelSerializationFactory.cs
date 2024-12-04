using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class KernelSerializationFactory : SerializationFactory<SerializableKernel, Kernel>
{
    public override SerializableKernel Serialize(Kernel original)
    {
        return new SerializableKernel
        {
            Width = original.Width,
            Height = original.Height,
            Values = original.AsSpan().ToArray()
        };    
    }

    public override bool TryDeserialize(object raw, out Kernel original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (raw is not Dictionary<string, object> serialized)
        {
            original = null;
            return false;
        }
        
        if (serialized.ContainsKey("Width") && serialized.ContainsKey("Height") && serialized.ContainsKey("Values"))
        {
            int width = ExtractInt(serialized["Width"]);
            int height = ExtractInt(serialized["Height"]);
            float[] values = ExtractArray<float>(serialized["Values"]);
            original = new Kernel(width, height, values);
            return true;
        }

        original = null;
        return false; 
    }

    public override string DeserializationId { get; } = "PixiEditor.Kernel";
}
