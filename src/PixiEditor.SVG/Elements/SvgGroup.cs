using System.Xml;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgGroup() : SvgElement("g"), ITransformable, IFillable, IStrokable, IElementContainer
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgColorUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgColorUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");

    public override void ParseData(XmlReader reader)
    {
        List<SvgProperty> properties = new List<SvgProperty>() { Transform, Fill, Stroke, StrokeWidth };
        ParseAttributes(properties, reader);
    }
}
