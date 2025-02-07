namespace PixiEditor.SVG.Units;

public enum SvgNumericUnits
{
    Px,
    In,
    Cm,
    Mm,
    Pt,
    Pc,
    Em,
    Ex,
    Ch,
    Rem,
    Vw,
    Vh,
    Vmin,
    Vmax,
    Percent,
    Deg,
    Rad,
    Grad,
    Turn,
    S,
    Ms,
    Min,
    H,
    Mmss,
    Hhmmss,
}

public static class SvgNumericConverter
{
    public static double? ToPixels(double value, SvgNumericUnits unit)
    {
        if (!unit.IsAbsoluteUnit() && !unit.IsSizeUnit()) return null;

        return unit switch
        {
            SvgNumericUnits.Px => value,
            SvgNumericUnits.In => value * 96,
            SvgNumericUnits.Cm => value * 37.795,
            SvgNumericUnits.Mm => value * 3.7795,
            SvgNumericUnits.Pt => value * 1.3333,
            SvgNumericUnits.Pc => value * 16,
            _ => null,
        };
    }

    public static double? FromPixels(double pixelsValue, SvgNumericUnits other)
    {
        if (!other.IsAbsoluteUnit() && !other.IsSizeUnit()) return null;

        return other switch
        {
            SvgNumericUnits.Px => pixelsValue,
            SvgNumericUnits.In => pixelsValue / 96,
            SvgNumericUnits.Cm => pixelsValue / 37.795,
            SvgNumericUnits.Mm => pixelsValue / 3.7795,
            SvgNumericUnits.Pt => pixelsValue / 1.3333,
            SvgNumericUnits.Pc => pixelsValue / 16,
            _ => null,
        };
    }
}

public static class SvgNumericUnitsExtensions
{
    public static bool IsSizeUnit(this SvgNumericUnits unit)
    {
        return unit switch
        {
            SvgNumericUnits.Px => true,
            SvgNumericUnits.In => true,
            SvgNumericUnits.Cm => true,
            SvgNumericUnits.Mm => true,
            SvgNumericUnits.Pt => true,
            SvgNumericUnits.Pc => true,
            SvgNumericUnits.Em => true,
            SvgNumericUnits.Ex => true,
            SvgNumericUnits.Ch => true,
            SvgNumericUnits.Rem => true,
            SvgNumericUnits.Vw => true,
            SvgNumericUnits.Vh => true,
            SvgNumericUnits.Vmin => true,
            SvgNumericUnits.Vmax => true,
            SvgNumericUnits.Percent => true,
            _ => false,
        };
    }

    public static bool IsAbsoluteUnit(this SvgNumericUnits unit)
    {
        return unit switch
        {
            SvgNumericUnits.Px => true,
            SvgNumericUnits.In => true,
            SvgNumericUnits.Cm => true,
            SvgNumericUnits.Mm => true,
            SvgNumericUnits.Pt => true,
            SvgNumericUnits.Pc => true,
            SvgNumericUnits.Rad => true,
            SvgNumericUnits.Deg => true,
            SvgNumericUnits.Grad => true,
            _ => false,
        };
    }

    public static SvgNumericUnits? TryParseUnit(string postFix)
    {
        if (string.IsNullOrWhiteSpace(postFix)) return SvgNumericUnits.Px;
        return postFix.ToLower().Trim() switch
        {
            "px" => SvgNumericUnits.Px,
            "in" => SvgNumericUnits.In,
            "cm" => SvgNumericUnits.Cm,
            "mm" => SvgNumericUnits.Mm,
            "pt" => SvgNumericUnits.Pt,
            "pc" => SvgNumericUnits.Pc,
            "em" => SvgNumericUnits.Em,
            "ex" => SvgNumericUnits.Ex,
            "ch" => SvgNumericUnits.Ch,
            "rem" => SvgNumericUnits.Rem,
            "vw" => SvgNumericUnits.Vw,
            "vh" => SvgNumericUnits.Vh,
            "vmin" => SvgNumericUnits.Vmin,
            "vmax" => SvgNumericUnits.Vmax,
            "%" => SvgNumericUnits.Percent,
            "deg" => SvgNumericUnits.Deg,
            "rad" => SvgNumericUnits.Rad,
            "grad" => SvgNumericUnits.Grad,
            "turn" => SvgNumericUnits.Turn,
            "s" => SvgNumericUnits.S,
            "ms" => SvgNumericUnits.Ms,
            "min" => SvgNumericUnits.Min,
            "h" => SvgNumericUnits.H,
            "mm:ss" => SvgNumericUnits.Mmss,
            "hh:mm:ss" => SvgNumericUnits.Hhmmss,
            _ => null,
        };
    }
}
