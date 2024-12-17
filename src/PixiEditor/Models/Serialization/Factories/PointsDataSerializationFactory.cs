using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class PointsDataSerializationFactory : VectorShapeSerializationFactory<PointsVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.PointsData";
    protected override void AddSpecificData(ByteBuilder builder, PointsVectorData original)
    {
        builder.AddVecDList(original.Points);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor,
        bool fill,
        Color fillColor,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out PointsVectorData original)
    {
        List<VecD> points = extractor.GetVecDList();
        original = new PointsVectorData(points)
        {
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix,
            Fill = fill
        };

        return true;
    }
}
