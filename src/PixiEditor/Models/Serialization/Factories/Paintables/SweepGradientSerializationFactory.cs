using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

internal class SweepGradientSerializationFactory : GradientPaintableSerializationFactory<SweepGradientPaintable>
{
    public override string DeserializationId { get; } = "PixiEditor.SweepGradientPaintable";
    protected override void SerializeSpecificGradient(SweepGradientPaintable paintable, ByteBuilder builder)
    {
        builder.AddVecD(paintable.Center);
        builder.AddDouble(paintable.Angle);
    }

    protected override SweepGradientPaintable DeserializeGradient(bool absoluteValues, Matrix3X3? transform, List<GradientStop> stops,
        ByteExtractor extractor)
    {
        VecD center = extractor.GetVecD();
        double angle = extractor.GetDouble();

        return new SweepGradientPaintable(center, angle, stops) { AbsoluteValues = absoluteValues, Transform = transform };
    }
}
