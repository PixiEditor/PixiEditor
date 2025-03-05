using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class PointsDataSerializationFactory : VectorShapeSerializationFactory<PointsVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.PointsData";
    protected override void AddSpecificData(ByteBuilder builder, PointsVectorData original)
    {
        builder.AddVecDList(original.Points);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out PointsVectorData original)
    {
        List<VecD> points = extractor.GetVecDList();
        original = new PointsVectorData(points)
        {
            Stroke = strokePaintable,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix,
            Fill = fill
        };

        return true;
    }
}
