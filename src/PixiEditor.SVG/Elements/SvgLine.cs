using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgLine() : SvgPrimitive("line") 
{
    public SvgProperty<SvgNumericUnit> X1 { get; } = new("x1");
    public SvgProperty<SvgNumericUnit> Y1 { get; } = new("y1");
    
    public SvgProperty<SvgNumericUnit> X2 { get; } = new("x2");
    public SvgProperty<SvgNumericUnit> Y2 { get; } = new("y2");
    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return X1;
        yield return Y1;
        yield return X2;
        yield return Y2;
    }
}
