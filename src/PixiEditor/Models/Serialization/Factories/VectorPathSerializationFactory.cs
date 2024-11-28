using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class VectorPathSerializationFactory : VectorShapeSerializationFactory<PathVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.PathData";

    protected override void AddSpecificData(ByteBuilder builder, PathVectorData original)
    {
        builder.AddString(original.Path.ToSvgPathData());
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor, Color fillColor,
        int strokeWidth, out PathVectorData original)
    {
        string path = extractor.GetString();

        original = new PathVectorData(VectorPath.FromSvgPath(path))
        {
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix
        };

        return true;
    }
}
