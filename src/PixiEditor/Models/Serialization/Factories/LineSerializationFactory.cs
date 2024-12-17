using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class LineSerializationFactory : VectorShapeSerializationFactory<LineVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.LineData";

    protected override void AddSpecificData(ByteBuilder builder, LineVectorData original)
    {
        builder.AddVecD(original.Start);
        builder.AddVecD(original.End);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor,
        Color fillColor,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out LineVectorData original)
    {
        VecD start = extractor.GetVecD();
        VecD end = extractor.GetVecD();

        original = new LineVectorData(start, end)
        {
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix
        };

        return true;
    }
}
