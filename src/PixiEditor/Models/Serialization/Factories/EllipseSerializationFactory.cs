using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class EllipseSerializationFactory : VectorShapeSerializationFactory<EllipseVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.EllipseData";

    protected override void AddSpecificData(ByteBuilder builder, EllipseVectorData original)
    {
        builder.AddVecD(original.Center);
        builder.AddVecD(original.Radius);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out EllipseVectorData original)
    {
        VecD center = extractor.GetVecD();
        VecD radius = extractor.GetVecD();

        original = new EllipseVectorData(center, radius)
        {
            Stroke = strokePaintable,
            Fill = fill,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix
        };

        return true;
    }
}
