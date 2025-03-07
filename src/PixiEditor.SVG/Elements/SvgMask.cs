using System.Xml;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgMask() : SvgElement("mask"), IElementContainer
{
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    
    public SvgProperty<SvgNumericUnit> Width { get; } = new("width");
    public SvgProperty<SvgNumericUnit> Height { get; } = new("height");
    public List<SvgElement> Children { get; } = new();

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>() { X, Y, Width, Height };
        ParseAttributes(properties, reader, defs);
    }
}
