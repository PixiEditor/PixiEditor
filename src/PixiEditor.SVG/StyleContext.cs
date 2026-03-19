using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public struct StyleContext
{
    public SvgProperty<SvgNumericUnit> StrokeWidth { get; }
    public SvgProperty<SvgPaintServerUnit> Stroke { get; }
    public SvgProperty<SvgNumericUnit> StrokeOpacity { get; }
    public SvgProperty<SvgPaintServerUnit> Fill { get; }
    public SvgProperty<SvgNumericUnit> FillOpacity { get; }
    public SvgProperty<SvgTransformUnit> Transform { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineCap>> StrokeLineCap { get; }
    public SvgProperty<SvgEnumUnit<SvgStrokeLineJoin>> StrokeLineJoin { get; }
    public SvgProperty<SvgPreserveAspectRatioUnit> PreserveAspectRatio { get; }
    public SvgProperty<SvgNumericUnit> FontSize { get; }
    public SvgProperty<SvgEnumUnit<SvgFontWeight>> FontWeight { get; }
    public SvgProperty<SvgEnumUnit<SvgFontStyle>> FontStyle { get; }
    public SvgProperty<SvgStringUnit> FontFamily { get; }
    public SvgProperty<SvgNumericUnit> Opacity { get; }
    public SvgProperty<SvgStyleUnit> InlineStyle { get; set; }
    public SvgProperty<SvgFilterUnit> Filter { get; }
    public VecD ViewboxOrigin { get; set; }
    public VecD ViewboxSize { get; set; }
    public VecD DocumentSize { get; set; }
    public SvgDefs Defs { get; set; }

    public StyleContext()
    {
        StrokeWidth = new("stroke-width");
        Stroke = new("stroke");
        StrokeOpacity = new("stroke-opacity");
        Fill = new("fill");
        FillOpacity = new("fill-opacity");
        Fill.Unit = SvgPaintServerUnit.FromColor(Colors.Black);
        Transform = new("transform");
        StrokeLineCap = new("stroke-linecap");
        StrokeLineJoin = new("stroke-linejoin");
        Opacity = new("opacity");
        InlineStyle = new("style");
        PreserveAspectRatio = new("preserve-aspect-ratio");
        FontSize = new("font-size");
        FontWeight = new("font-weight");
        FontStyle = new("font-style");
        FontFamily = new("font-family");
        Filter = new("filter");
        Defs = new();
    }

    public StyleContext(SvgDocument document)
    {
        StrokeWidth = FallbackToCssStyle(document.StrokeWidth, document.Style);
        Stroke = FallbackToCssStyle(document.Stroke, document.Style);
        StrokeOpacity = FallbackToCssStyle(document.StrokeOpacity, document.Style);
        Fill = FallbackToCssStyle(document.Fill, document.Style, SvgPaintServerUnit.FromColor(Colors.Black));
        FillOpacity = FallbackToCssStyle(document.FillOpacity, document.Style);
        Transform = FallbackToCssStyle(document.Transform, document.Style, new SvgTransformUnit(Matrix3X3.Identity));
        StrokeLineCap = FallbackToCssStyle(document.StrokeLineCap, document.Style);
        StrokeLineJoin = FallbackToCssStyle(document.StrokeLineJoin, document.Style);
        Opacity = FallbackToCssStyle(document.Opacity, document.Style);
        FontSize = FallbackToCssStyle(document.FontSize, document.Style);
        FontWeight = FallbackToCssStyle(document.FontWeight, document.Style);
        FontStyle = FallbackToCssStyle(document.FontStyle, document.Style);
        FontFamily = FallbackToCssStyle(document.FontFamily, document.Style);
        Filter = FallbackToCssStyle(document.Filter, document.Style);
        PreserveAspectRatio = document.PreserveAspectRatio.Unit.HasValue ?
            document.PreserveAspectRatio :
            FallbackToCssStyle(document.PreserveAspectRatio, document.Style, new SvgPreserveAspectRatioUnit(SvgAspectRatio.XMidYMid, SvgMeetOrSlice.Meet));
        ViewboxOrigin = new VecD(
            document.ViewBox.Unit.HasValue ? -document.ViewBox.Unit.Value.Value.X : document.X.Unit?.Value ?? 0,
            document.ViewBox.Unit.HasValue ? -document.ViewBox.Unit.Value.Value.Y : document.Y.Unit?.Value ?? 0);
        ViewboxSize = new VecD(
            document.ViewBox.Unit.HasValue ? document.ViewBox.Unit.Value.Value.Width : document.Width.Unit?.Value ?? 0,
            document.ViewBox.Unit.HasValue ? document.ViewBox.Unit.Value.Value.Height : document.Height.Unit?.Value ?? 0);
        DocumentSize = new VecD(
            document.Width.Unit?.Value ?? ViewboxSize.X,
            document.Height.Unit?.Value ?? ViewboxSize.Y);
        InlineStyle = document.Style;
        Defs = document.Defs;
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
                styleContext.InlineStyle, SvgPaintServerUnit.FromColor(Colors.Black)).Unit;
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

            styleContext.StrokeOpacity.Unit =
                FallbackToCssStyle(strokableElement.StrokeOpacity, styleContext.StrokeOpacity, styleContext.InlineStyle).Unit;
        }

        if (element is IOpacity opacityElement)
        {
            styleContext.Opacity.Unit =
                FallbackToCssStyle(opacityElement.Opacity, styleContext.Opacity, styleContext.InlineStyle).Unit;
        }

        if (element is ITextData textData)
        {
            styleContext.FontSize.Unit =
                FallbackToCssStyle(textData.FontSize, styleContext.FontSize, styleContext.InlineStyle).Unit;
            styleContext.FontWeight.Unit =
                FallbackToCssStyle(textData.FontWeight, styleContext.FontWeight, styleContext.InlineStyle).Unit;
            styleContext.FontStyle.Unit =
                FallbackToCssStyle(textData.FontStyle, styleContext.FontStyle, styleContext.InlineStyle).Unit;
            styleContext.FontFamily.Unit =
                FallbackToCssStyle(textData.FontFamily, styleContext.FontFamily, styleContext.InlineStyle).Unit;
        }

        if (element is IFilterable filterableElement)
        {
            styleContext.Filter.Unit =
                FallbackToCssStyle(filterableElement.Filter, styleContext.Filter, styleContext.InlineStyle).Unit;
        }

        if (element is SvgDocument doc)
        {
            styleContext.ViewboxOrigin = new VecD(
                doc.ViewBox.Unit.HasValue ? -doc.ViewBox.Unit.Value.Value.X : styleContext.ViewboxOrigin.X,
                doc.ViewBox.Unit.HasValue ? -doc.ViewBox.Unit.Value.Value.Y : styleContext.ViewboxOrigin.Y);
            styleContext.ViewboxSize = new VecD(
                doc.ViewBox.Unit.HasValue ? doc.ViewBox.Unit.Value.Value.Width : styleContext.ViewboxSize.X,
                doc.ViewBox.Unit.HasValue ? doc.ViewBox.Unit.Value.Value.Height : styleContext.ViewboxSize.Y);

            styleContext.DocumentSize = new VecD(
                doc.Width.Unit?.Value ?? styleContext.ViewboxSize.X,
                doc.Height.Unit?.Value ?? styleContext.ViewboxSize.Y);

            styleContext.PreserveAspectRatio.Unit = doc.PreserveAspectRatio.Unit ?? FallbackToCssStyle(doc.PreserveAspectRatio, styleContext.PreserveAspectRatio, styleContext.InlineStyle,
                new SvgPreserveAspectRatioUnit(SvgAspectRatio.XMidYMid, SvgMeetOrSlice.Meet)).Unit;
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
            styleContext.Stroke.Unit = new SvgPaintServerUnit(Stroke.Unit.Value.Paintable.Clone()) { LinksTo = Stroke.Unit.Value.LinksTo };
        }

        if (StrokeOpacity.Unit != null)
        {
            styleContext.StrokeOpacity.Unit = StrokeOpacity.Unit;
        }

        if (Fill.Unit != null)
        {
            styleContext.Fill.Unit = new SvgPaintServerUnit(Fill.Unit.Value.Paintable.Clone()) { LinksTo = Fill.Unit.Value.LinksTo };
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
        styleContext.ViewboxSize = ViewboxSize;
        styleContext.DocumentSize = DocumentSize;

        if (InlineStyle.Unit != null)
        {
            styleContext.InlineStyle.Unit = InlineStyle.Unit;
        }

        if (PreserveAspectRatio.Unit != null)
        {
            styleContext.PreserveAspectRatio.Unit = PreserveAspectRatio.Unit;
        }

        if (FontSize.Unit != null)
        {
            styleContext.FontSize.Unit = FontSize.Unit;
        }

        if (FontWeight.Unit != null)
        {
            styleContext.FontWeight.Unit = FontWeight.Unit;
        }

        if (FontStyle.Unit != null)
        {
            styleContext.FontStyle.Unit = FontStyle.Unit;
        }

        if (FontFamily.Unit != null)
        {
            styleContext.FontFamily.Unit = FontFamily.Unit;
        }

        styleContext.Defs = Defs;

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
        return style?.TryGetStyleFor<SvgProperty<TUnit>, TUnit>(property.SvgName, Defs)
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
        var styleProp = style?.TryGetStyleFor<SvgProperty<TUnit>, TUnit>(property.SvgName, Defs);
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
