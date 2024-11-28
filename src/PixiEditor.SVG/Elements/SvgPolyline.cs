using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPolyline() : SvgPrimitive("polyline")
{
    public SvgArray<SvgNumericUnit> Points { get; } = new SvgArray<SvgNumericUnit>("points");
}
