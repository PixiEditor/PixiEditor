using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface IStrokable
{
    public SvgProperty<SvgColorUnit> Stroke { get; }
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
}
