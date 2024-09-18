using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class PointsDataSerializationFactory : VectorShapeSerializationFactory<PointsVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.PointsData";
    protected override void AddSpecificData(ByteBuilder builder, PointsVectorData original)
    {
        builder.AddVecDList(original.Points);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor, Color fillColor,
        int strokeWidth, out PointsVectorData original)
    {
        List<VecD> points = extractor.GetVecDList();
        original = new PointsVectorData(points)
        {
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix
        };

        return true;
    }
}
