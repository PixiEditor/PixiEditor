using System.Runtime.CompilerServices;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

// TODO: Might wanna write that for the 4x5 matrix too
public class ColorMatrixFactory : SerializationFactory<byte[], ColorMatrix>
{
    public override string DeserializationId { get; } = "PixiEditor.ColorMatrix";
    
    public override byte[] Serialize(ColorMatrix original)
    {
        var members = original.ToArray();
        var bytes = new byte[ColorMatrix.Width * ColorMatrix.Height * sizeof(float)];

        Buffer.BlockCopy(members, 0, bytes, 0, bytes.Length);

        return bytes;
    }

    public override bool TryDeserialize(object serialized, out ColorMatrix original)
    {
        if (serialized is not byte[] bytes)
        {
            original = default;
            return false;
        }

        var members = new float[ColorMatrix.Width * ColorMatrix.Height];
        
        Buffer.BlockCopy(bytes, 0, members, 0, bytes.Length);
        original = ColorMatrix.CreateFromMembers(members);

        return true;
    }
}
