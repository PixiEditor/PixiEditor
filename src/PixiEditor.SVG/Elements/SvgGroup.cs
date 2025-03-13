using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgGroup()
    : SvgElement("g"), ITransformable, IFillable, IStrokable, IOpacity, IElementContainer, IDefsStorage
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgPaintServerUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgNumericUnit> FillOpacity { get; } = new("fill-opacity");
    public SvgProperty<SvgPaintServerUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; } = new("stroke-linecap");
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; } = new("stroke-linejoin");
    public SvgProperty<SvgNumericUnit> Opacity { get; } = new("opacity");
    public SvgDefs Defs { get; } = new();

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            Transform,
            Fill,
            Stroke,
            StrokeWidth,
            StrokeLineCap,
            StrokeLineJoin
        };

        ParseAttributes(properties, reader, defs); // TODO: merge with Defs?
    }
}
