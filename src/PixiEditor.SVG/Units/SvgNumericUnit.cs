using System.Globalization;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgNumericUnit(double value, string postFix) : ISvgUnit
{
    public string PostFix { get; set; } = postFix;
    public double Value { get; set; } = value;
    public double? PixelsValue => ConvertTo(SvgNumericUnits.Px);

    public double? ToPixels(RectD viewBox)
    {
        SvgNumericUnits? numericUnit = SvgNumericUnitsExtensions.TryParseUnit(PostFix);
        if (numericUnit == null || !numericUnit.Value.IsSizeUnit())
        {
            return Value;
        }

        double? pixelsValue = SvgNumericConverter.ToPixels(Value, numericUnit.Value, viewBox);
        if (pixelsValue == null)
        {
            return null;
        }

        return pixelsValue.Value;
    }

    public double? ConvertTo(SvgNumericUnits other)
    {
        SvgNumericUnits? numericUnit = SvgNumericUnitsExtensions.TryParseUnit(PostFix);

        if (numericUnit == null || !numericUnit.Value.IsSizeUnit() || !numericUnit.Value.IsAbsoluteUnit())
        {
            return null;
        }

        double? pixelsValue = SvgNumericConverter.ToPixels(Value, numericUnit.Value);
        if (pixelsValue == null)
        {
            return null;
        }

        return SvgNumericConverter.FromPixels(pixelsValue.Value, other);
    }

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

    public string ToXml(DefStorage defs)
    {
        string invariantValue = Value.ToString(CultureInfo.InvariantCulture);
        return $"{invariantValue}{PostFix}";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        string? extractedPostFix = ExtractPostFix(readerValue);

        if (extractedPostFix == null)
        {
            if (double.TryParse(readerValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                Value = result;
                PostFix = string.Empty;
            }
        }
        else
        {
            string value = readerValue.Substring(0, readerValue.Length - extractedPostFix.Length);
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                Value = result;
                PostFix = extractedPostFix;
            }
        }
    }

    private string? ExtractPostFix(string readerValue)
    {
        if (readerValue.Length == 0)
        {
            return null;
        }

        int postFixStartIndex = readerValue.Length;

        if (char.IsDigit(readerValue[^1]))
        {
            return null;
        }

        for (int i = readerValue.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(readerValue[i]))
            {
                postFixStartIndex = i + 1;
                break;
            }
        }


        return readerValue.Substring(postFixStartIndex);
    }

    public double NormalizedValue(bool clamp = true)
    {
        double value = Value;
        if (PostFix == "%")
        {
            value /= 100;
        }

        if (clamp)
        {
            value = Math.Clamp(value, 0, 1);
        }

        return value;
    }
}
