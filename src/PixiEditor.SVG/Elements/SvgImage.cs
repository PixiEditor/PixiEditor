using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgImage : SvgElement
{
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    
    public SvgProperty<SvgNumericUnit> Width { get; } = new("width");
    public SvgProperty<SvgNumericUnit> Height { get; } = new("height");
    
    public SvgProperty<SvgStringUnit> Href { get; } = new("xlink:href");
    
    public SvgImage() : base("image")
    {
        RequiredNamespaces.Add("xlink", "http://www.w3.org/1999/xlink");
    }
}
