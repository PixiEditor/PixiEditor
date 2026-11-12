using System.Xml;
using PixiEditor.SVG.Attributes;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements.Filters;

public class SvgFeConvolveMatrix() : SvgFilterPrimitive("feConvolveMatrix"), IImageFilter
{
    public SvgProperty<SvgNumericArrayUnit> KernelMatrix { get; } = new("kernelMatrix");
    public SvgProperty<SvgVectorUnit> Order { get; } = new("order");
    public SvgProperty<SvgNumericUnit> Divisor { get; } = new("divisor");
    public SvgProperty<SvgNumericUnit> Bias { get; } = new("bias");
    public SvgProperty<SvgNumericUnit> TargetX { get; } = new("targetX");
    public SvgProperty<SvgNumericUnit> TargetY { get; } = new("targetY");
    public SvgProperty<SvgVectorUnit> KernelUnitLength { get; } = new("kernelUnitLength");
    public SvgProperty<SvgBoolUnit> PreserveAlpha { get; } = new("preserveAlpha");

    public override void ParseAttributes(XmlReader reader, SvgDefs defs)
    {
        List<SvgProperty> properties = new List<SvgProperty>()
        {
            X, Y, Width, Height,
            KernelMatrix, Order, Divisor, Bias,
            TargetX, TargetY, KernelUnitLength, PreserveAlpha
        };

        ParseAttributes(properties, reader, defs);
    }
    public SvgFilterPrimitive? GetImageFilter()
    {
        return this;
    }
}
