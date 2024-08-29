using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class PointsDataSerializationFactory : SerializationFactory<byte[], PointsData>
{
    public override string DeserializationId { get; } = "PixiEditor.PointsData";
    public override byte[] Serialize(PointsData original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddVecDList(original.Points);
        builder.AddColor(original.FillColor);
        builder.AddInt(original.StrokeWidth);
        
        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out PointsData original)
    {
        if (serialized is not byte[] data)
        {
            original = null;
            return false;
        }
        
        ByteExtractor extractor = new ByteExtractor(data);
        
        List<VecD> points = extractor.GetVecDList();
        Color fillColor = extractor.GetColor();
        int strokeWidth = extractor.GetInt();
        
        original = new PointsData(points)
        {
            FillColor = fillColor,
            StrokeWidth = strokeWidth
        };
        
        return true;
    }
}
