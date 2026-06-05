using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;

namespace PixiEditor.Models.Serialization.Factories;

public class PaletteSerializationFactory : SerializationFactory<Byte[], Palette>
{
    public override string DeserializationId { get; } = "PixiEditor.Palette";
    public override byte[] Serialize(Palette original)
    {
        return original.SelectMany(c => new byte[] {c.R, c.G, c.B, c.A }).ToArray();
    }

    public override bool TryDeserialize(object serialized, out Palette original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is byte[] bytes && bytes.Length % 4 == 0)
        {
            original = new Palette(bytes.Chunk(4).Select(b => new Color(b[0], b[1], b[2], b[3])));
            return true;
        }

        original = default;
        return false;
    }
}
