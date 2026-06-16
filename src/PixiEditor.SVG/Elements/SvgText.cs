using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgText() : SvgPrimitive("text"), ITextData
{
    public SvgProperty<SvgStringUnit> Text { get; } = new("");
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    public SvgProperty<SvgNumericUnit> FontSize { get; } = new("font-size");
    public SvgProperty<SvgStringUnit> FontFamily { get; } = new("font-family");
    public SvgProperty<SvgEnumUnit<SvgFontWeight>> FontWeight { get; } = new("font-weight");
    public SvgProperty<SvgEnumUnit<SvgFontStyle>> FontStyle { get; } = new("font-style");
    public SvgProperty<SvgEnumUnit<SvgTextAnchor>> TextAnchor { get; } = new("text-anchor");

    public override void ParseElement(XmlReader reader, SvgDefs defs)
    {
        base.ParseElement(reader, defs);
        reader.MoveToElement();
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
        yield return TextAnchor;
    }

    private string ParseContent(XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Element || reader.Name != "text")
            return string.Empty;

        if (reader.IsEmptyElement)
        {
            return string.Empty;
        }

        string content = string.Empty;

        if (reader.NodeType == XmlNodeType.None) return content;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
            {
                content = reader.Value?.Replace("\r\n", "\n").Replace("\r", "\n") ?? string.Empty;
                content = Regex.Replace(content, @"[\t\n ]+", " ");
            }
            else if (reader is { NodeType: XmlNodeType.Element})
            {
                reader.Read();
            }
            else if (reader is { NodeType: XmlNodeType.EndElement, Name: "text" })
            {
                break;
            }
        }

        return content;
    }
}
