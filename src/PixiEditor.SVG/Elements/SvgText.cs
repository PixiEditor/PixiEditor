using System.Xml;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgText() : SvgPrimitive("text")
{
    public SvgProperty<SvgStringUnit> Text { get; } = new("");
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");

    public override void ParseData(XmlReader reader)
    {
        base.ParseData(reader);
        Text.Unit = new SvgStringUnit(ParseContent(reader));
    }

    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return X;
        yield return Y;
    }

    private string ParseContent(XmlReader reader)
    {
        string content = string.Empty;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Text)
            {
                content = reader.Value;
            }
            else if (reader is { NodeType: XmlNodeType.EndElement, Name: "text" })
            {
                break;
            }
        }

        return content;
    }
}
