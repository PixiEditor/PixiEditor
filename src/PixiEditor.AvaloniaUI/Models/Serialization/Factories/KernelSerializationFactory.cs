using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

public class KernelSerializationFactory : SerializationFactory<SerializableKernel, Kernel>
{
    public KernelSerializationFactory(SerializationConfig config) : base(config)
    {
    }

    public override SerializableKernel Serialize(Kernel original)
    {
        return new SerializableKernel
        {
            Width = original.Width,
            Height = original.Height,
            Values = original.AsSpan().ToArray()
        };    
    }

    public override Kernel Deserialize(SerializableKernel serialized)
    {
        Kernel kernel = new(serialized.Width, serialized.Height, serialized.Values);
        return kernel;
    }
}
