using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface IFilterable
{
    public SvgProperty<SvgFilterUnit> Filter { get; }
}
