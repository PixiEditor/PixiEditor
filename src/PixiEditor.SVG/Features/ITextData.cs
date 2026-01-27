using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface ITextData
{
    public SvgProperty<SvgStringUnit> FontFamily { get; }
    public SvgProperty<SvgNumericUnit> FontSize { get; }
    public SvgProperty<SvgEnumUnit<SvgFontWeight>> FontWeight { get; }
    public SvgProperty<SvgEnumUnit<SvgFontStyle>> FontStyle { get; }
}
