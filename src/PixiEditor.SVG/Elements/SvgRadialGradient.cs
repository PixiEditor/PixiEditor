using System.Xml;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgRadialGradient() : SvgElement("radialGradient"), IElementContainer, IPaintServer
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransformUnit> GradientTransform { get; } = new("gradientTransform");
    public SvgProperty<SvgNumericUnit> Cx { get; } = new("cx");
    public SvgProperty<SvgNumericUnit> Cy { get; } = new("cy");
    public SvgProperty<SvgNumericUnit> R { get; } = new("r");
    public SvgProperty<SvgNumericUnit> Fx { get; } = new("fx");
    public SvgProperty<SvgNumericUnit> Fy { get; } = new("fy");
    public SvgProperty<SvgEnumUnit<SvgSpreadMethod>> SpreadMethod { get; } = new("spreadMethod");
    public SvgProperty<SvgEnumUnit<SvgRelativityUnit>> GradientUnits { get; } = new("gradientUnits");

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = GetProperties().ToList();

        do
        {
            ParseAttributes(properties, reader, defs);
        } while (reader.MoveToNextAttribute());
    }

    protected IEnumerable<SvgProperty> GetProperties()
    {
        yield return GradientTransform;
        yield return Cx;
        yield return Cy;
        yield return R;
        yield return Fx;
        yield return Fy;
        yield return SpreadMethod;
        yield return GradientUnits;
    }

    public Paintable GetPaintable()
    {
        VecD center = new VecD(GetUnit(Cx)?.Value ?? 0.5, GetUnit(Cy)?.Value ?? 0.5);
        //VecD focus = new VecD(Fx.Unit.Value.PixelsValue ?? 0, Fy.Unit.Value.PixelsValue ?? 0);
        double radius = GetUnit(R)?.Value ?? 0.5;

        List<GradientStop> gradientStops = new();
        foreach (SvgElement child in Children)
        {
            if (child is SvgStop stop)
            {
                Color color = stop.GetUnit(stop.StopColor)?.Color ?? Colors.Black;
                color = color.WithAlpha((byte)((stop.StopOpacity.Unit?.NormalizedValue() ?? 1) * 255));
                gradientStops.Add(
                    new GradientStop(color, stop.GetUnit(stop.Offset)?.NormalizedValue() ?? 0));
            }
        }

        var unit = GetUnit(GradientUnits)?.Value ?? SvgRelativityUnit.ObjectBoundingBox;
        var transform = GetUnit(GradientTransform)?.MatrixValue ?? Matrix3X3.Identity;

        RadialGradientPaintable radialGradientPaintable =
            new(center, radius, gradientStops)
            {
                AbsoluteValues = unit == SvgRelativityUnit.UserSpaceOnUse,
                Transform = transform
            };

        return radialGradientPaintable;
    }
}
