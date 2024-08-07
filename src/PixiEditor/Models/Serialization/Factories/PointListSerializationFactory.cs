using MessagePack;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

namespace PixiEditor.Models.Serialization.Factories;

public class PointListSerializationFactory : SerializationFactory<byte[], PointList>
{
    public override string DeserializationId { get; } = "PixiEditor.PointList";

    public override byte[] Serialize(PointList original)
    {
        return MessagePackSerializer.Serialize(original);
    }

    public override bool TryDeserialize(object serialized, out PointList? original)
    {
        if (serialized is not byte[] buffer)
        {
            original = null;
            return false;
        }
        
        original = MessagePackSerializer.Deserialize<PointList>(buffer);

        return true;
    }
}
