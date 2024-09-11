using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgLinkUnit : ISvgUnit
{
    public string? ObjectReference { get; set; } 
    public string ToXml()
    {
        return ObjectReference != null ? $"url(#{ObjectReference}" : string.Empty;
    }

    public static SvgLinkUnit FromElement(SvgElement element)
    {
        return new SvgLinkUnit
        {
            ObjectReference = element.Id.Unit?.Value
        };
    }
}
