using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

internal class RectangleSerializationFactory : VectorShapeSerializationFactory<RectangleVectorData> 
{
    public override string DeserializationId { get; } = "PixiEditor.RectangleData";


    protected override void AddSpecificData(ByteBuilder builder, RectangleVectorData original)
    {
        builder.AddVecD(original.Center);
        builder.AddVecD(original.Size);
        builder.AddDouble(original.CornerRadius);
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out RectangleVectorData original)
    {
        VecD center = extractor.GetVecD();
        VecD size = extractor.GetVecD();
        double cornerRadius = 0;
        if (!IsFilePreVersion(serializerData, new Version(2, 0, 0, 81)))
        {
            cornerRadius = extractor.GetDouble();
        }

        original = new RectangleVectorData(center, size)
        {
            Stroke = strokePaintable,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix,
            Fill = fill,
            CornerRadius = cornerRadius
        };

        return true;
    }
}
