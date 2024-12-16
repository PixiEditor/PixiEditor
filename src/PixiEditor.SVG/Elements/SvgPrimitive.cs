using System.Xml;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public abstract class SvgPrimitive(string tagName) : SvgElement(tagName), ITransformable, IFillable, IStrokable
{
    public SvgProperty<SvgTransformUnit> Transform { get; } = new("transform");
    public SvgProperty<SvgColorUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgColorUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
    public override void ParseData(XmlReader reader)
    {
        List<SvgProperty> properties = GetProperties().ToList();

        if(!reader.MoveToFirstAttribute())
        {
            return;
        }
        
        do
        {
            ParseAttributes(properties, reader); 
        } while (reader.MoveToNextAttribute());
    }
    
    protected abstract IEnumerable<SvgProperty> GetProperties();
}
