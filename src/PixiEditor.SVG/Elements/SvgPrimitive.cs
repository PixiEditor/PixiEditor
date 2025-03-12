using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public abstract class SvgPrimitive(string tagName) : SvgElement(tagName), ITransformable, IFillable, IStrokable, IOpacity
{
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgPaintServerUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgNumericUnit> FillOpacity { get; } = new("fill-opacity");
    public SvgProperty<SvgPaintServerUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; } = new("stroke-linecap");
    
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; } = new("stroke-linejoin");

    public SvgProperty<SvgNumericUnit> Opacity { get; } = new("opacity");

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = GetProperties().ToList();
        
        properties.Add(Transform);
        properties.Add(Fill);
        properties.Add(FillOpacity);
        properties.Add(Stroke);
        properties.Add(StrokeWidth);
        properties.Add(StrokeLineCap);
        properties.Add(StrokeLineJoin);
        properties.Add(Opacity);

        do
        {
            ParseAttributes(properties, reader, defs);
        } while (reader.MoveToNextAttribute());
    }

    protected abstract IEnumerable<SvgProperty> GetProperties();
}
