using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgClipPath() : SvgElement("clipPath"), IElementContainer
{
    public List<SvgElement> Children { get; } = new();

    public SvgProperty<SvgEnumUnit<SvgRelativityUnit>> ClipPathUnits { get; } = new("clipPathUnits");

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>() { ClipPathUnits };
        ParseAttributes(properties, reader, defs);
    }
}
