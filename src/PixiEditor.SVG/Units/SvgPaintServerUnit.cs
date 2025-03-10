using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;

namespace PixiEditor.SVG.Units;

public struct SvgPaintServerUnit : ISvgUnit
{
    public Paintable Paintable { get; set; }

    public SvgLinkUnit? LinksTo { get; set; } = null;

    public SvgPaintServerUnit(Paintable paintable)
    {
        Paintable = paintable;
    }

    public static SvgPaintServerUnit FromColor(Color color)
    {
        return new SvgPaintServerUnit(new ColorPaintable(color));
    }

    public string ToXml(DefStorage defs)
    {
        if (LinksTo != null)
        {
            return LinksTo.Value.ToXml(defs);
        }

        return TrySerialize(Paintable, defs);
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        var linkUnit = new SvgLinkUnit();
        linkUnit.ValuesFromXml(readerValue, defs);
        LinksTo = linkUnit;
        if (string.IsNullOrEmpty(LinksTo.Value.ObjectReference))
        {
            LinksTo = null;
            SvgColorUnit colorUnit = new SvgColorUnit();
            colorUnit.ValuesFromXml(readerValue, defs);
            Paintable = new ColorPaintable(colorUnit.Color);
        }
        else
        {
            if (defs.TryFindElement(LinksTo.Value.ObjectReference, out SvgElement? element) &&
                element is IPaintServer server)
            {
                Paintable = server.GetPaintable();
            }
        }
    }

    private string TrySerialize(Paintable paintable, DefStorage defs)
    {
        if (paintable is ColorPaintable colorPaintable)
        {
            return colorPaintable.Color.ToRgbHex();
        }

        if (paintable is GradientPaintable gradientPaintable)
        {
            return TrySerializeGradient(gradientPaintable, defs);
        }

        return "";
    }

    private string TrySerializeGradient(GradientPaintable gradientPaintable, DefStorage defs)
    {
        switch (gradientPaintable)
        {
            case LinearGradientPaintable linearGradientPaintable:
                return CreateLinearGradient(linearGradientPaintable, defs);
            case RadialGradientPaintable radialGradientPaintable:
                return CreateRadialGradient(radialGradientPaintable, defs);
            default:
                return "";
        }
    }

    private string CreateLinearGradient(LinearGradientPaintable linearGradientPaintable, DefStorage defs)
    {
        SvgLinearGradient linearGradient = new SvgLinearGradient();
        linearGradient.Id.Unit = new SvgStringUnit($"linearGradient{defs.GetNextId()}");
        linearGradient.X1.Unit = new SvgNumericUnit(linearGradientPaintable.Start.X, "");
        linearGradient.Y1.Unit = new SvgNumericUnit(linearGradientPaintable.Start.Y, "");
        linearGradient.X2.Unit = new SvgNumericUnit(linearGradientPaintable.End.X, "");
        linearGradient.Y2.Unit = new SvgNumericUnit(linearGradientPaintable.End.Y, "");
        if (linearGradientPaintable.AbsoluteValues)
        {
            linearGradient.GradientUnits.Unit = new SvgEnumUnit<SvgRelativityUnit>(SvgRelativityUnit.UserSpaceOnUse);
        }

        if (linearGradientPaintable.Transform is { IsIdentity: false })
        {
            linearGradient.GradientTransform.Unit = new SvgTransformUnit(linearGradientPaintable.Transform.Value);
        }

        foreach (var stop in linearGradientPaintable.GradientStops)
        {
            SvgStop svgStop = new SvgStop();
            svgStop.Offset.Unit = new SvgNumericUnit(stop.Offset * 100, "%");
            svgStop.StopColor.Unit = new SvgColorUnit(stop.Color.ToRgbHex());
            svgStop.StopOpacity.Unit = new SvgNumericUnit(stop.Color.A / 255.0, "");
            linearGradient.Children.Add(svgStop);
        }

        defs.AddDef(linearGradient);
        return $"url(#{linearGradient.Id.Unit.Value.Value})";
    }

    private string CreateRadialGradient(RadialGradientPaintable radialGradientPaintable, DefStorage defs)
    {
        SvgRadialGradient radialGradient = new SvgRadialGradient();
        radialGradient.Id.Unit = new SvgStringUnit($"radialGradient{defs.GetNextId()}");
        radialGradient.Cx.Unit = new SvgNumericUnit(radialGradientPaintable.Center.X, "");
        radialGradient.Cy.Unit = new SvgNumericUnit(radialGradientPaintable.Center.Y, "");
        radialGradient.R.Unit = new SvgNumericUnit(radialGradientPaintable.Radius, "");
        if (radialGradientPaintable.AbsoluteValues)
        {
            radialGradient.GradientUnits.Unit = new SvgEnumUnit<SvgRelativityUnit>(SvgRelativityUnit.UserSpaceOnUse);
        }

        if (radialGradientPaintable.Transform is { IsIdentity: false })
        {
            radialGradient.GradientTransform.Unit = new SvgTransformUnit(radialGradientPaintable.Transform.Value);
        }

        foreach (var stop in radialGradientPaintable.GradientStops)
        {
            SvgStop svgStop = new SvgStop();
            svgStop.Offset.Unit = new SvgNumericUnit(stop.Offset * 100, "%");
            svgStop.StopColor.Unit = new SvgColorUnit(stop.Color.ToRgbHex());
            svgStop.StopOpacity.Unit = new SvgNumericUnit(stop.Color.A / 255.0, "");
            radialGradient.Children.Add(svgStop);
        }

        defs.AddDef(radialGradient);
        return $"url(#{radialGradient.Id.Unit.Value.Value})";
    }
}
