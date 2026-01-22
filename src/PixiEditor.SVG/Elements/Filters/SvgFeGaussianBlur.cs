using System.Xml;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeGaussianBlur() : SvgFilterPrimitive("feGaussianBlur"), IImageFilter
{
    public SvgProperty<SvgNumericUnit> StdDeviation { get; } = new("stdDeviation");
    public SvgProperty<SvgEnumUnit<SvgEdgeMode>> EdgeMode { get; } = new("edgeMode");


    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            StdDeviation
        };

        ParseAttributes(properties, reader, defs);
    }

    public ImageFilter? GetImageFilter()
    {
        float stdDev = (float)(StdDeviation.Unit?.Value ?? 0);
        SvgEdgeMode edgeMode = EdgeMode.Unit?.Value ?? SvgEdgeMode.None;
        return ImageFilter.CreateBlur(stdDev, stdDev, ConvertEdgeMode(edgeMode));
    }

    private TileMode ConvertEdgeMode(SvgEdgeMode edgeMode)
    {
        return edgeMode switch
        {
            SvgEdgeMode.None => TileMode.Clamp,
            SvgEdgeMode.Wrap => TileMode.Repeat,
            SvgEdgeMode.Duplicate => TileMode.Mirror,
            _ => TileMode.Clamp,
        };
    }
}
