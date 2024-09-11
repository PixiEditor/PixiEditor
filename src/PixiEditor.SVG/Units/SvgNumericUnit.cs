using System.Globalization;

namespace PixiEditor.SVG.Units;

public struct SvgNumericUnit(double value, string postFix) : ISvgUnit
{
    public string PostFix { get; } = postFix;
    public double Value { get; set; } = value;

    public static SvgNumericUnit FromUserUnits(double value)
    {
        return new SvgNumericUnit(value, string.Empty);
    }
    
    public static SvgNumericUnit FromPixels(double value)
    {
        return new SvgNumericUnit(value, "px");
    }
    
    public static SvgNumericUnit FromInches(double value)
    {
        return new SvgNumericUnit(value, "in");
    }
    
    public static SvgNumericUnit FromCentimeters(double value)
    {
        return new SvgNumericUnit(value, "cm");
    }
    
    public static SvgNumericUnit FromMillimeters(double value)
    {
        return new SvgNumericUnit(value, "mm");
    }
    
    public static SvgNumericUnit FromPercent(double value)
    {
        return new SvgNumericUnit(value, "%");
    }

    public string ToXml()
    {
        string invariantValue = Value.ToString(CultureInfo.InvariantCulture);
        return $"{invariantValue}{PostFix}";
    }
}
