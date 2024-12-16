using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgRectangle() : SvgPrimitive("rect")
{
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");

    public SvgProperty<SvgNumericUnit> Width { get; } = new("width"); 
    public SvgProperty<SvgNumericUnit> Height { get; } = new("height"); 
    
    public SvgProperty<SvgNumericUnit> Rx { get; } = new("rx");
    public SvgProperty<SvgNumericUnit> Ry { get; } = new("ry");
    
    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return X;
        yield return Y;
        yield return Width;
        yield return Height;
        yield return Rx;
        yield return Ry;
    }
}
