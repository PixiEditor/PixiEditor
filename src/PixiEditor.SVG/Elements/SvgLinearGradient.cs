using System.Xml;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgLinearGradient() : SvgElement("linearGradient"), IElementContainer, IPaintServer
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransformUnit> GradientTransform { get; } = new("gradientTransform");
    public SvgProperty<SvgNumericUnit> X1 { get; } = new("x1");
    public SvgProperty<SvgNumericUnit> Y1 { get; } = new("y1");
    public SvgProperty<SvgNumericUnit> X2 { get; } = new("x2");
    public SvgProperty<SvgNumericUnit> Y2 { get; } = new("y2");
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
        yield return X1;
        yield return Y1;
        yield return X2;
        yield return Y2;
        yield return SpreadMethod;
        yield return GradientUnits;
    }

    public Paintable GetPaintable()
    {
        VecD start = new VecD(GetUnit(X1)?.Value ?? 0, GetUnit(Y1)?.Value ?? 0.5);
        VecD end = new VecD(GetUnit(X2)?.Value ?? 1, GetUnit(Y2)?.Value ?? 0.5);

        List<GradientStop> gradientStops = new();
        foreach (SvgElement child in Children)
        {
            if (child is SvgStop stop)
            {
                Color color = stop.GetUnit(stop.StopColor)?.Color ?? Colors.Black;
                color = color.WithAlpha((byte)((stop.StopOpacity.Unit?.NormalizedValue() ?? 1) * 255));
                gradientStops.Add(
                    new GradientStop(color, GetUnit(stop.Offset)?.NormalizedValue() ?? 0));
            }
        }

        var unit = GetUnit(GradientUnits)?.Value ?? SvgRelativityUnit.ObjectBoundingBox;
        var transform = GetUnit(GradientTransform)?.MatrixValue ?? Matrix3X3.Identity;

        return new LinearGradientPaintable(start, end, gradientStops)
        {
            AbsoluteValues = unit == SvgRelativityUnit.UserSpaceOnUse,
            Transform = transform
        };
    }
}
