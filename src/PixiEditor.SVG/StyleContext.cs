using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public struct StyleContext
{
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
    public SvgProperty<SvgColorUnit> Stroke { get; }
    public SvgProperty<SvgColorUnit> Fill { get; }
    public SvgProperty<SvgTransformUnit> Transform { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; }
    public SvgProperty<SvgNumericUnit> Opacity { get; }

    public StyleContext()
    {
        StrokeWidth = new("stroke-width");
        Stroke = new("stroke");
        Fill = new("fill");
        Fill.Unit = new SvgColorUnit?(new SvgColorUnit("black"));
        Transform = new("transform");
        StrokeLineCap = new("stroke-linecap");
        StrokeLineJoin = new("stroke-linejoin");
        Opacity = new("opacity");
    }
    
    public StyleContext(SvgDocument document)
    {
        StrokeWidth = document.StrokeWidth;
        Stroke = document.Stroke;
        Fill = document.Fill;
        Transform = document.Transform;
        StrokeLineCap = document.StrokeLineCap;
        StrokeLineJoin = document.StrokeLineJoin;
        Opacity = document.Opacity;
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
            
            if (strokableElement.StrokeLineCap.Unit != null)
            {
                styleContext.StrokeLineCap.Unit = strokableElement.StrokeLineCap.Unit;
            }
            
            if (strokableElement.StrokeLineJoin.Unit != null)
            {
                styleContext.StrokeLineJoin.Unit = strokableElement.StrokeLineJoin.Unit;
            }
        }

        if(element is IOpacity opacityElement)
        {
            styleContext.Opacity.Unit = opacityElement.Opacity.Unit;
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

        if (StrokeLineCap.Unit != null)
        {
            styleContext.StrokeLineCap.Unit = StrokeLineCap.Unit;
        }

        if (StrokeLineJoin.Unit != null)
        {
            styleContext.StrokeLineJoin.Unit = StrokeLineJoin.Unit;
        }

        if (Opacity.Unit != null)
        {
            styleContext.Opacity.Unit = Opacity.Unit;
        }

        return styleContext;
    }
}
