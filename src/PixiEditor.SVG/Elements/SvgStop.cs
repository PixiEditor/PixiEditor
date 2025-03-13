using System.Xml;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgStop() : SvgElement("stop")
{
    public SvgProperty<SvgColorUnit> StopColor { get; } = new("stop-color");
    public SvgProperty<SvgNumericUnit> Offset { get; } = new("offset");
    public SvgProperty<SvgNumericUnit> StopOpacity { get; } = new("stop-opacity");

    public override void ParseData(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = GetProperties().ToList();

        do
        {
            ParseAttributes(properties, reader, defs);
        } while (reader.MoveToNextAttribute());
    }

    private IEnumerable<SvgProperty> GetProperties()
    {
        yield return StopColor;
        yield return Offset;
        yield return StopOpacity;
    }
}
