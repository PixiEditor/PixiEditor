using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class EllipseSerializationFactory : SerializationFactory<byte[], EllipseData>
{
    public override string DeserializationId { get; } = "PixiEditor.EllipseData";
    public override byte[] Serialize(EllipseData original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddVecD(original.Center);
        builder.AddVecD(original.Radius);
        builder.AddColor(original.StrokeColor);
        builder.AddColor(original.FillColor);
        builder.AddInt(original.StrokeWidth);
        
        return builder.Build();
    }

    public override bool TryDeserialize(object serialized, out EllipseData original)
    {
        if (serialized is not byte[] data)
        {
            original = null;
            return false;
        }
        
        ByteExtractor extractor = new ByteExtractor(data);
        
        VecD center = extractor.GetVecD();
        VecD radius = extractor.GetVecD();
        Color strokeColor = extractor.GetColor();
        Color fillColor = extractor.GetColor();
        int strokeWidth = extractor.GetInt();
        
        original = new EllipseData(center, radius)
        {
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeWidth = strokeWidth
        };
        
        return true;
    }
}
