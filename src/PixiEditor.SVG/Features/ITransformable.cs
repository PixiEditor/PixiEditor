using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface ITransformable
{
    public SvgProperty<SvgTransformUnit> Transform { get; }
}
