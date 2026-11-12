using PixiEditor.SVG.Attributes;

namespace PixiEditor.SVG.Enums;

public enum SvgFontWeight
{
    [SvgValue("100")]
    Lighter = 100,
    [SvgValue("400")]
    Normal = 400,
    [SvgValue("700")]
    Bold = 700,
    [SvgValue("900")]
    Bolder = 900,
}
