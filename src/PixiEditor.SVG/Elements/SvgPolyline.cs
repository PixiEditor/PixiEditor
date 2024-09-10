using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPolyline : SvgPrimitive
{
    public SvgArray<SvgNumericUnit> Points { get; } = new SvgArray<SvgNumericUnit>("points");
}
