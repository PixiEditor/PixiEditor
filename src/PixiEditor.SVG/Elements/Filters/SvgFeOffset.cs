using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeOffset() : SvgFilterPrimitive("feOffset"), IImageFilter
{
    public SvgProperty<SvgNumericUnit> Dx { get; } = new("dx");
    public SvgProperty<SvgNumericUnit> Dy { get; } = new("dy");

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            Dx, Dy
        };

        ParseAttributes(properties, reader, defs);
    }

    public SvgFilterPrimitive? GetImageFilter()
    {
        return this;
    }
}
