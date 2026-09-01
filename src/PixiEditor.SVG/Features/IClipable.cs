using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Features;

public interface IClipable
{
    public SvgProperty<SvgStringUnit> ClipPath { get; }
}
