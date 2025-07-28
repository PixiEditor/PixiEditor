using PixiEditor.SVG.Attributes;

namespace PixiEditor.SVG.Enums;

public enum SvgFillRule
{
    [SvgValue("nonzero")]
    NonZero,
    
    [SvgValue("evenodd")]
    EvenOdd
}
