using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPath() : SvgPrimitive("path")
{
    public SvgProperty<SvgStringUnit> PathData { get; } = new("d");
    public SvgProperty<SvgEnumUnit<SvgFillRule>> FillRule { get; } = new("fill-rule");

    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return PathData;
        yield return FillRule;
    }
}
