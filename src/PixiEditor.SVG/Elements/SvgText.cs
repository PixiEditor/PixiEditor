using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgText() : SvgPrimitive("text")
{
    public SvgProperty<SvgStringUnit> Text { get; } = new("");
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    public SvgProperty<SvgNumericUnit> FontSize { get; } = new("font-size");
    public SvgProperty<SvgStringUnit> FontFamily { get; } = new("font-family");
    public SvgProperty<SvgEnumUnit<SvgFontWeight>> FontWeight { get; } = new("font-weight");
    public SvgProperty<SvgEnumUnit<SvgFontStyle>> FontStyle { get; } = new("font-style");

    public override void ParseData(XmlReader reader)
    {
        base.ParseData(reader);
        Text.Unit = new SvgStringUnit(ParseContent(reader));
    }

    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return X;
        yield return Y;
        yield return FontSize;
        yield return FontFamily;
        yield return FontWeight;
        yield return FontStyle;
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
