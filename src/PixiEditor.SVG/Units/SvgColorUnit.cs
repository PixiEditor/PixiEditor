using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Exceptions;
using PixiEditor.SVG.Utils;

namespace PixiEditor.SVG.Units;

public struct SvgColorUnit : ISvgUnit
{
    private string value;

    public string Value
    {
        get => value;
        set
        {
            this.value = value;
            if(SvgColorUtility.TryConvertStringToColor(value, out Color color))
            {
                Color = color;
            }
        }
    }
    public Color Color { get; private set; }

    public SvgColorUnit(string value)
    {
        Value = value;
    }

    public static SvgColorUnit FromHex(string value)
    {
        return new SvgColorUnit(value);
    }

    public static SvgColorUnit FromRgb(int r, int g, int b)
    {
        return new SvgColorUnit($"rgb({r},{g},{b})");
    }

    public static SvgColorUnit FromRgba(int r, int g, int b, double a)
    {
        return new SvgColorUnit($"rgba({r},{g},{b},{a})");
    }

    public static SvgColorUnit FromHsl(int h, int s, int l)
    {
        return new SvgColorUnit($"hsl({h},{s}%,{l}%)");
    }

    public static SvgColorUnit FromHsla(int h, int s, int l, double a)
    {
        return new SvgColorUnit($"hsla({h},{s}%,{l}%,{a})");
    }

    public string ToXml(DefStorage defs)
    {
        return Value;
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        Value = readerValue;
    }
}

public enum SvgColorType
{
    Hex,
    Rgb,
    Rgba,
    Hsl,
    Hsla,
    Named
}
