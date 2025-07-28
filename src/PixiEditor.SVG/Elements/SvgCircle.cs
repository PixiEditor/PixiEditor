using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgCircle() : SvgPrimitive("circle")
{
    public SvgProperty<SvgNumericUnit> Cx { get; } = new("cx");
    public SvgProperty<SvgNumericUnit> Cy { get; } = new("cy");

    public SvgProperty<SvgNumericUnit> R { get; } = new("r");
    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return Cx;
        yield return Cy;
        yield return R;
    }
}
