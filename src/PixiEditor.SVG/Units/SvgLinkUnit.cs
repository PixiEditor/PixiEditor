using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgLinkUnit : ISvgUnit
{
    public string? ObjectReference { get; set; } 
    public string ToXml()
    {
        return ObjectReference != null ? $"url(#{ObjectReference}" : string.Empty;
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        if (readerValue.StartsWith("url(#") && readerValue.EndsWith(')'))
        {
            ObjectReference = readerValue[5..^1];
        }
    }

    public static SvgLinkUnit FromElement(SvgElement element)
    {
        return new SvgLinkUnit
        {
            ObjectReference = element.Id.Unit?.Value
        };
    }
}
