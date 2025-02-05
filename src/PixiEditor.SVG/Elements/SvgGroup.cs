using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgGroup() : SvgElement("g"), ITransformable, IFillable, IStrokable, IOpacity, IElementContainer
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgColorUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgColorUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; } = new("stroke-linecap");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; } = new("stroke-linejoin");
    public SvgProperty<SvgNumericUnit> Opacity { get; } = new("opacity");

    public override void ParseData(XmlReader reader)
    {
        List<SvgProperty> properties = new List<SvgProperty>() { Transform, Fill, Stroke, StrokeWidth, StrokeLineCap, StrokeLineJoin };
        ParseAttributes(properties, reader);
    }

}
