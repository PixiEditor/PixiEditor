using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgG : SvgElement, ITransformable, IFillable, IStrokable
{
    public List<SvgElement> Children { get; } = new();
    public SvgProperty<SvgTransform> Transform { get; } = new("transform");
    public SvgProperty<SvgColorUnit> Fill { get; } = new("fill");
    public SvgProperty<SvgColorUnit> Stroke { get; } = new("stroke");
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; } = new("stroke-width");
}
