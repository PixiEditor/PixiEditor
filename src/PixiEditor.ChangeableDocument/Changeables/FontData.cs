using Drawie.Backend.Core.Text;

namespace PixiEditor.ChangeableDocument.Changeables;

public record struct FontData
{
    public double Size { get; set; }
    public FontFamilyName Family { get; set; }
    public bool SubPixel { get; set; }
    public FontEdging Edging { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }

    public FontData(FontFamilyName family)
    {
        Size = 12;
        Family = family;
        SubPixel = true;
        Edging = FontEdging.AntiAlias;
        Bold = false;
        Italic = false;
    }

    public static FontData CreateDefault()
    {
        return new FontData(new FontFamilyName("$Default"));
    }

    public Font? ToFont(bool defaultFallback = true)
    {
        Font font = Font.FromFontFamily(Family);
        if (font == null)
        {
            if (defaultFallback)
            {
                font = Font.FromFontFamily(new FontFamilyName("$Default"));
                if (font == null)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        font.Size = Size;
        font.SubPixel = SubPixel;
        font.Edging = Edging;
        font.Bold = Bold;
        font.Italic = Italic;
        return font;
    }
}
