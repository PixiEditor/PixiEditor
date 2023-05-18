namespace PixiEditor.Extensions.Palettes;

public struct PaletteColor
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public string Hex => $"#{R:X2}{G:X2}{B:X2}";

    public PaletteColor(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public override string ToString()
    {
        return Hex;
    }
}
