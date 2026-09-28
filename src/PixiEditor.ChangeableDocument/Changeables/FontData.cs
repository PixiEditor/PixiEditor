using Drawie.Backend.Core;
using Drawie.Backend.Core.Text;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables;

public record struct FontData : ICacheable
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
                font = Font.FromFontFamily(new FontFamilyName("$Default")) ?? Font.CreateDefault();
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

    public int GetCacheHash()
    {
        HashCode code = new HashCode();
        code.Add(Size);
        code.Add(SubPixel);
        code.Add(Edging);
        code.Add(Bold);
        code.Add(Italic);
        code.Add(Family.Name);
        code.Add(Family.FontUri != null ? 1 : 0);
        if (Family.FontUri != null)
        {
            code.Add(Family.FontUri.AbsolutePath);
        }

        return code.ToHashCode();
    }
}
