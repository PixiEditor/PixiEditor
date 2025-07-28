using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

internal class LinearGradientSerializationFactory : GradientPaintableSerializationFactory<LinearGradientPaintable>
{
    public override string DeserializationId { get; } = "PixiEditor.LinearGradientPaintable";


    protected override void SerializeSpecificGradient(LinearGradientPaintable paintable, ByteBuilder builder)
    {
        builder.AddVecD(paintable.Start);
        builder.AddVecD(paintable.End);
    }

    protected override LinearGradientPaintable DeserializeGradient(bool absoluteValues, Matrix3X3? transform,
        List<GradientStop> stops, ByteExtractor extractor)
    {
        VecD start = extractor.GetVecD();
        VecD end = extractor.GetVecD();

        return new LinearGradientPaintable(start, end, stops) { AbsoluteValues = absoluteValues, Transform = transform };
    }
}
