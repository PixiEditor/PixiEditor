using System.Xml;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public abstract class SvgFilterPrimitive(string tagName) : SvgElement(tagName)
{
    public SvgProperty<SvgNumericUnit> X { get; } = new("x");
    public SvgProperty<SvgNumericUnit> Y { get; } = new("y");
    public SvgProperty<SvgNumericUnit> Width { get; } = new("width");
    public SvgProperty<SvgNumericUnit> Height { get; } = new("height");

}
