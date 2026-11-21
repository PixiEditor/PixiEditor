using System.Xml;
using PixiEditor.SVG.Attributes;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgFilter() : SvgElement("filter"), IElementContainer, IImageFilter
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgEnumUnit<SvgFilterUnits>> FilterUnits { get; } = new("filterUnits");

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            Id, FilterUnits
        };

        ParseAttributes(properties, reader, defs);
    }

    public SvgFilterPrimitive? GetImageFilter()
    {
        SvgFilterPrimitive? lastFilter = null;
        foreach (var child in Children)
        {
            if (child is IImageFilter filter)
            {
                lastFilter = filter.GetImageFilter();
            }
        }

        return lastFilter;
    }
}

public enum SvgFilterUnits
{
    [SvgValue("objectBoundingBox")]
    ObjectBoundingBox,
    [SvgValue("userSpaceOnUse")]
    UserSpaceOnUse
}
