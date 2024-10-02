using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgEllipse() : SvgPrimitive("ellipse")
{
    public SvgProperty<SvgNumericUnit> Cx { get; } = new("cx");
    public SvgProperty<SvgNumericUnit> Cy { get; } = new("cy"); 
    
    public SvgProperty<SvgNumericUnit> Rx { get; } = new("rx"); 
    public SvgProperty<SvgNumericUnit> Ry { get; } = new("ry"); 
}
