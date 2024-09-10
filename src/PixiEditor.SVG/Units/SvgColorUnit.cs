namespace PixiEditor.SVG.Units;

public struct SvgColorUnit : ISvgUnit
{
    public string Value { get; set; }

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
}
