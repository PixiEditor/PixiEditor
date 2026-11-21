using PixiEditor.SVG.Attributes;

namespace PixiEditor.SVG.Enums;

public enum SvgFontWidth
{
    [SvgValue("50%")]
    UltraCondensed = 1,
    [SvgValue("62.5%")]
    ExtraCondensed = 2,
    [SvgValue("75%")]
    Condensed = 3,
    [SvgValue("87.5%")]
    SemiCondensed = 4,
    [SvgValue("100%")]
    Normal = 5,
    [SvgValue("112.5%")]
    SemiExpanded = 6,
    [SvgValue("125%")]
    Expanded = 7,
    [SvgValue("150%")]
    ExtraExpanded = 8,
    [SvgValue("200%")]
    UltraExpanded = 9,
}
