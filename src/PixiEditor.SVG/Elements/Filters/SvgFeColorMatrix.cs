using System.Xml;
using PixiEditor.SVG.Attributes;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeColorMatrix() : SvgFilterPrimitive("feColorMatrix"), IImageFilter
{
    public SvgProperty<SvgNumericArrayUnit> Values { get; } = new("values");
    public SvgProperty<SvgEnumUnit<SvgColorMatrixType>> Type { get; } = new("type");

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            Values, Type
        };

        ParseAttributes(properties, reader, defs);
    }
    public SvgFilterPrimitive? GetImageFilter()
    {
        return this;
    }
}

public enum SvgColorMatrixType
{
    Matrix,
    Saturate,
    [SvgValue("hueRotate")]
    HueRotate,
    [SvgValue("luminanceToAlpha")]
    luminanceToAlpha
}
