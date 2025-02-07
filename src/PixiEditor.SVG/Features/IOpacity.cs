using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface IOpacity
{
    public SvgProperty<SvgNumericUnit> Opacity { get; }
}
