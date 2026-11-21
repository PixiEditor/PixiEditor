using System.Xml;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeMorphology() : SvgFilterPrimitive("feMorphology"), IImageFilter
{
    public SvgProperty<SvgVectorUnit> Radius { get; } = new("radius");
    public SvgProperty<SvgEnumUnit<SvgMorphologyOperator>> Operator { get; } = new("operator");

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            Radius, Operator
        };

        ParseAttributes(properties, reader, defs);
    }

    public SvgFilterPrimitive? GetImageFilter()
    {
        return this;
    }
}

public enum SvgMorphologyOperator
{
    Erode,
    Dilate
}
