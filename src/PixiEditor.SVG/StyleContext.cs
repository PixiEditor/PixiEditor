using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public struct StyleContext
{
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
    public SvgProperty<SvgColorUnit> Stroke { get; }
    public SvgProperty<SvgColorUnit> Fill { get; }
    public SvgProperty<SvgTransformUnit> Transform { get; }

    public StyleContext()
    {
        StrokeWidth = new("stroke-width");
        Stroke = new("stroke");
        Fill = new("fill");
        Transform = new("transform");
    }
    
    public StyleContext(SvgDocument document)
    {
        StrokeWidth = document.StrokeWidth;
        Stroke = document.Stroke;
        Fill = document.Fill;
        Transform = document.Transform;
    }

    public StyleContext WithElement(SvgElement element)
    {
        StyleContext styleContext = Copy();

        if (element is ITransformable { Transform.Unit: not null } transformableElement)
        {
            styleContext.Transform.Unit = transformableElement.Transform.Unit;
        }

        if (element is IFillable { Fill.Unit: not null } fillableElement)
        {
            styleContext.Fill.Unit = fillableElement.Fill.Unit;
        }

        if (element is IStrokable strokableElement)
        {
            if (strokableElement.Stroke.Unit != null)
            {
                styleContext.Stroke.Unit = strokableElement.Stroke.Unit;
            }

            if (strokableElement.StrokeWidth.Unit != null)
            {
                styleContext.StrokeWidth.Unit = strokableElement.StrokeWidth.Unit;
            }
        }

        return styleContext;
    }

    private StyleContext Copy()
    {
        StyleContext styleContext = new();
        if (StrokeWidth.Unit != null)
        {
            styleContext.StrokeWidth.Unit = StrokeWidth.Unit;
        }

        if (Stroke.Unit != null)
        {
            styleContext.Stroke.Unit = Stroke.Unit;
        }

        if (Fill.Unit != null)
        {
            styleContext.Fill.Unit = Fill.Unit;
        }

        if (Transform.Unit != null)
        {
            styleContext.Transform.Unit = Transform.Unit;
        }

        return styleContext;
    }
}
