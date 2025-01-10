using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.SVG.Exceptions;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Utils;

public static class SvgColorUtility
{
    public static SvgColorType ResolveColorType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new SvgParsingException("Color value is empty");
        }

        if (value.StartsWith('#'))
        {
            return SvgColorType.Hex;
        }

        if (value.StartsWith("rgb"))
        {
            return value.Length > 3 && value[3] == 'a' ? SvgColorType.Rgba : SvgColorType.Rgb;
        }

        if (value.StartsWith("hsl"))
        {
            return value.Length > 3 && value[3] == 'a' ? SvgColorType.Hsla : SvgColorType.Hsl;
        }

        return SvgColorType.Named;
    }

    public static float[]? ExtractColorValues(string readerValue, SvgColorType colorType)
    {
        if (colorType == SvgColorType.Hex)
        {
            return [];
        }

        if (colorType == SvgColorType.Named)
        {
            return [];
        }

        string[] values = readerValue.Split(',', '(', ')').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        float[] colorValues = new float[values.Length - 1];
        for (int i = 1; i < values.Length; i++)
        {
            if (colorType is SvgColorType.Rgba or SvgColorType.Hsla)
            {
                if (i == values.Length - 1)
                {
                    if (float.TryParse(values[i], out float alpha))
                    {
                        if (alpha > 0 && alpha < 1)
                        {
                            colorValues[i - 1] = (byte)(alpha * 255);
                        }
                        else
                        {
                            colorValues[i - 1] = Math.Clamp(alpha, 0, 255);
                        }

                        continue;
                    }

                    throw new SvgParsingException($"Could not parse alpha value: {values[i]}");
                }
            }
            
            if(colorType is SvgColorType.Hsl or SvgColorType.Hsla)
            {
                if (values[i].EndsWith('%'))
                {
                    values[i] = values[i][..^1];
                }
                
                if (float.TryParse(values[i], out float result))
                {
                    int clampMax = i == 1 ? 360 : 100;
                    colorValues[i - 1] = Math.Clamp(result, 0, clampMax);
                }
            }
            else if (int.TryParse(values[i], out int result))
            {
                int clamped = Math.Clamp(result, 0, 255);
                colorValues[i - 1] = (byte)clamped;
            }
            else
            {
                throw new SvgParsingException($"Could not parse color value: {values[i]}");
            }
        }

        return colorValues;
    }

    public static bool TryConvertStringToColor(string input, out Color color)
    {
        try
        {
            if(input == "none")
            {
                color = Colors.Transparent;
                return true;
            }
            
            SvgColorType colorType = ResolveColorType(input);
            float[]? values = ExtractColorValues(input, colorType);
            int requiredValues = colorType switch
            {
                SvgColorType.Rgb => 3,
                SvgColorType.Rgba => 4,
                SvgColorType.Hsl => 3,
                SvgColorType.Hsla => 4,
                _ => 0
            };

            if (values == null || values.Length != requiredValues)
            {
                color = default;
                return false;
            }

            color = colorType switch
            {
                SvgColorType.Hex => Color.FromHex(input),
                SvgColorType.Named => Color.FromHex(
                    WellKnownColorNames.NamedToHexMap.GetValueOrDefault(input, "#000000")),
                SvgColorType.Rgb => Color.FromRgb((byte)values![0], (byte)values[1], (byte)values[2]),
                SvgColorType.Rgba => Color.FromRgba((byte)values![0], (byte)values[1], (byte)values[2], (byte)values[3]),
                SvgColorType.Hsl => Color.FromHsl(values![0], values[1], values[2]),
                SvgColorType.Hsla => Color.FromHsla(values![0], values[1], values[2], (byte)values[3]),
                _ => default
            };
        }
        catch (SvgParsingException)
        {
            color = default;
            return false;
        }
        
        return true;
    }
}
