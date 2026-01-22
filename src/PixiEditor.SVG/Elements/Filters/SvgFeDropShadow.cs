using System.Xml;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeDropShadow() : SvgFilterPrimitive("feDropShadow"), IImageFilter
{
    public SvgProperty<SvgNumericUnit> Dx { get; } = new("dx");
    public SvgProperty<SvgNumericUnit> Dy { get; } = new("dy");
    public SvgProperty<SvgNumericUnit> StdDeviation { get; } = new("stdDeviation");
    public SvgProperty<SvgColorUnit> FloodColor { get; } = new("flood-color");
    public SvgProperty<SvgNumericUnit> FloodOpacity { get; } = new("flood-opacity");


    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            Dx, Dy, StdDeviation, FloodColor, FloodOpacity
        };

        ParseAttributes(properties, reader, defs);
    }

    public SvgFilterPrimitive? GetImageFilter()
    {
        return this;
    }
}
