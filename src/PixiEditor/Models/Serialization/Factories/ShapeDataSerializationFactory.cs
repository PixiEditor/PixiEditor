using MessagePack;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

namespace PixiEditor.Models.Serialization.Factories;

public class ShapeDataSerializationFactory : SerializationFactory<byte[], ShapeData>
{
    public override string DeserializationId { get; } = "PixiEditor.PointList";

    public override byte[] Serialize(ShapeData original)
    {
        return MessagePackSerializer.Serialize(original);
    }

    public override bool TryDeserialize(object serialized, out ShapeData? original)
    {
        if (serialized is not byte[] buffer)
        {
            original = null;
            return false;
        }
        
        original = MessagePackSerializer.Deserialize<ShapeData>(buffer);

        return true;
    }
}
