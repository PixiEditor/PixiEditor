using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
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

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out LineVectorData original)
    {
        VecD start = extractor.GetVecD();
        VecD end = extractor.GetVecD();

        original = new LineVectorData(start, end)
        {
            Stroke = strokePaintable,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix
        };

        return true;
    }
}
