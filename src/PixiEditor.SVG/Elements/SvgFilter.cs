using System.Xml;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG.Elements;

public class SvgFilter() : SvgElement("filter"), IElementContainer, IImageFilter
{
    public List<SvgElement> Children { get; } = new();

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            Id,
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
