using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

internal class RadialGradientSerializationFactory : GradientPaintableSerializationFactory<RadialGradientPaintable>
{
    public override string DeserializationId { get; } = "PixiEditor.RadialGradientPaintable";
    protected override void SerializeSpecificGradient(RadialGradientPaintable paintable, ByteBuilder builder)
    {
        builder.AddVecD(paintable.Center);
        builder.AddDouble(paintable.Radius);
    }

    protected override RadialGradientPaintable DeserializeGradient(bool absoluteValues, Matrix3X3? transform, List<GradientStop> stops,
        ByteExtractor extractor)
    {
        VecD center = extractor.GetVecD();
        double radius = extractor.GetDouble();

        return new RadialGradientPaintable(center, radius, stops) { AbsoluteValues = absoluteValues, Transform = transform };
    }
}
