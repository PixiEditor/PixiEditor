using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;

namespace PixiEditor.Models.Serialization.Factories;

public class PaletteSerializationFactory : SerializationFactory<byte[], Palette>
{
    public override string DeserializationId { get; } = "PixiEditor.Palette";
    public override byte[] Serialize(Palette original)
    {
         int count = original.Count;
         byte[] result = new byte[count * 4];
         for (int i = 0; i < count; i++)
         {
             Color c = original[i];
             int o = i * 4;
             result[o] = c.R;
             result[o + 1] = c.G;
             result[o + 2] = c.B;
             result[o + 3] = c.A;
         }
         return result;
    }

    public override bool TryDeserialize(object serialized, out Palette original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes && bytes.Length % 4 == 0)
        {
             int count = bytes.Length / 4;
             Color[] colors = new Color[count];
             for (int i = 0; i < count; i++)
             {
                 int o = i * 4;
                 colors[i] = new Color(bytes[o], bytes[o + 1], bytes[o + 2], bytes[o + 3]);
             }
             original = new Palette(colors);
             return true;
        }

        original = default;
        return false;
    }
}
