using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface IStrokable
{
    public SvgProperty<SvgPaintServerUnit> Stroke { get; }
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; }
}
