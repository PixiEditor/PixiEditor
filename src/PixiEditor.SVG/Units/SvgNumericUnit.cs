using System.Globalization;

namespace PixiEditor.SVG.Units;

public struct SvgNumericUnit(double value, string postFix) : ISvgUnit
{
    public string PostFix { get; set; } = postFix;
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

    public void ValuesFromXml(string readerValue)
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
        
        for (int i = readerValue.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(readerValue[i]) && readerValue[i] != '.')
            {
                postFixStartIndex = i + 1;
                break;
            }
        }
        
        if (postFixStartIndex == readerValue.Length)
        {
            return null;
        }
        
        return readerValue.Substring(postFixStartIndex);
    }
}
