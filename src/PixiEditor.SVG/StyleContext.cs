using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public struct StyleContext
{
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
    public SvgProperty<SvgColorUnit> Stroke { get; }
    public SvgProperty<SvgColorUnit> Fill { get; }
    public SvgProperty<SvgNumericUnit> FillOpacity { get; }
    public SvgProperty<SvgTransformUnit> Transform { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; }
    public SvgProperty<SvgNumericUnit> Opacity { get; }
    public SvgProperty<SvgStyleUnit> InlineStyle { get; set; }
    public VecD ViewboxOrigin { get; set; }

    public StyleContext()
    {
        StrokeWidth = new("stroke-width");
        Stroke = new("stroke");
        Fill = new("fill");
        FillOpacity = new("fill-opacity");
        Fill.Unit = new SvgColorUnit?(new SvgColorUnit("black"));
        Transform = new("transform");
        StrokeLineCap = new("stroke-linecap");
        StrokeLineJoin = new("stroke-linejoin");
        Opacity = new("opacity");
        InlineStyle = new("style");
    }

    public StyleContext(SvgDocument document)
    {
        StrokeWidth = FallbackToCssStyle(document.StrokeWidth, document.Style);
        Stroke = FallbackToCssStyle(document.Stroke, document.Style);
        Fill = FallbackToCssStyle(document.Fill, document.Style, new SvgColorUnit("black"));
        FillOpacity = FallbackToCssStyle(document.FillOpacity, document.Style);
        Transform = FallbackToCssStyle(document.Transform, document.Style, new SvgTransformUnit(Matrix3X3.Identity));
        StrokeLineCap = FallbackToCssStyle(document.StrokeLineCap, document.Style);
        StrokeLineJoin = FallbackToCssStyle(document.StrokeLineJoin, document.Style);
        Opacity = FallbackToCssStyle(document.Opacity, document.Style);
        ViewboxOrigin = new VecD(
            document.ViewBox.Unit.HasValue ? -document.ViewBox.Unit.Value.Value.X : 0,
            document.ViewBox.Unit.HasValue ? -document.ViewBox.Unit.Value.Value.Y : 0);
        InlineStyle = document.Style;
    }

    public StyleContext WithElement(SvgElement element)
    {
        StyleContext styleContext = Copy();

        styleContext.InlineStyle = MergeInlineStyle(element.Style, InlineStyle);

        if (element is ITransformable transformableElement)
        {
            if (styleContext.Transform.Unit == null)
            {
                styleContext.Transform.Unit =
                    FallbackToCssStyle(transformableElement.Transform, styleContext.Transform, styleContext.InlineStyle)
                        .Unit;
            }
            else
            {
                styleContext.Transform.Unit = new SvgTransformUnit(
                    styleContext.Transform.Unit.Value.MatrixValue.Concat(
                        FallbackToCssStyle(transformableElement.Transform, styleContext.InlineStyle).Unit
                            ?.MatrixValue ??
                        Matrix3X3.Identity));
            }
        }

        if (element is IFillable fillableElement)
        {
            styleContext.Fill.Unit = FallbackToCssStyle(fillableElement.Fill, styleContext.Fill,
                styleContext.InlineStyle, new SvgColorUnit("black")).Unit;
            styleContext.FillOpacity.Unit =
                FallbackToCssStyle(fillableElement.FillOpacity, styleContext.FillOpacity, styleContext.InlineStyle)
                    .Unit;
        }

        if (element is IStrokable strokableElement)
        {
            styleContext.Stroke.Unit =
                FallbackToCssStyle(strokableElement.Stroke, styleContext.Stroke, styleContext.InlineStyle).Unit;

            styleContext.StrokeWidth.Unit =
                FallbackToCssStyle(strokableElement.StrokeWidth, styleContext.StrokeWidth, styleContext.InlineStyle)
                    .Unit;

            styleContext.StrokeLineCap.Unit =
                FallbackToCssStyle(strokableElement.StrokeLineCap, styleContext.StrokeLineCap, styleContext.InlineStyle)
                    .Unit;

            styleContext.StrokeLineJoin.Unit =
                FallbackToCssStyle(strokableElement.StrokeLineJoin, styleContext.StrokeLineJoin,
                    styleContext.InlineStyle).Unit;
        }

        if (element is IOpacity opacityElement)
        {
            styleContext.Opacity.Unit =
                FallbackToCssStyle(opacityElement.Opacity, styleContext.Opacity, styleContext.InlineStyle).Unit;
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

        if (FillOpacity.Unit != null)
        {
            styleContext.FillOpacity.Unit = FillOpacity.Unit;
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

        styleContext.ViewboxOrigin = ViewboxOrigin;

        if (InlineStyle.Unit != null)
        {
            styleContext.InlineStyle.Unit = InlineStyle.Unit;
        }

        return styleContext;
    }


    private SvgProperty<TUnit>? FallbackToCssStyle<TUnit>(
        SvgProperty<TUnit> property,
        SvgProperty<SvgStyleUnit> inlineStyle, TUnit? fallback = null) where TUnit : struct, ISvgUnit
    {
        if (property.Unit != null)
        {
            return property;
        }

        SvgStyleUnit? style = inlineStyle.Unit;
        return style?.TryGetStyleFor<SvgProperty<TUnit>, TUnit>(property.SvgName)
               ?? (fallback.HasValue
                   ? new SvgProperty<TUnit>(property.SvgName) { Unit = fallback.Value }
                   : new SvgProperty<TUnit>(property.SvgName));
    }

    private SvgProperty<TUnit>? FallbackToCssStyle<TUnit>(
        SvgProperty<TUnit> property,
        SvgProperty<TUnit> parentStyleProperty,
        SvgProperty<SvgStyleUnit> inlineStyle, TUnit? fallback = null) where TUnit : struct, ISvgUnit
    {
        if (property.Unit != null)
        {
            return property;
        }

        SvgStyleUnit? style = inlineStyle.Unit;
        var styleProp = style?.TryGetStyleFor<SvgProperty<TUnit>, TUnit>(property.SvgName);
        if (styleProp != null) return styleProp;
        if(parentStyleProperty.Unit != null)
        {
            return parentStyleProperty;
        }

        return (fallback.HasValue
            ? new SvgProperty<TUnit>(property.SvgName) { Unit = fallback.Value }
            : new SvgProperty<TUnit>(property.SvgName));
    }

    private SvgProperty<SvgStyleUnit> MergeInlineStyle(SvgProperty<SvgStyleUnit> elementStyle,
        SvgProperty<SvgStyleUnit> parentStyle)
    {
        SvgStyleUnit? elementStyleUnit = elementStyle.Unit;
        SvgStyleUnit? parentStyleUnit = parentStyle.Unit;

        if (elementStyleUnit == null)
        {
            return parentStyle;
        }

        if (parentStyleUnit == null)
        {
            return elementStyle;
        }

        SvgStyleUnit style = parentStyleUnit.Value.MergeWith(elementStyleUnit.Value);
        return new SvgProperty<SvgStyleUnit>("style") { Unit = style };
    }
}
