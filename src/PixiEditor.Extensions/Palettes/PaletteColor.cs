namespace PixiEditor.Extensions.Palettes;

public struct PaletteColor
{
    public static PaletteColor Empty => new(0, 0, 0);
    public static PaletteColor Black => new(0, 0, 0);
    public static PaletteColor White => new(255, 255, 255);
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

    public static bool operator ==(PaletteColor left, PaletteColor right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B;
    }

    public static bool operator !=(PaletteColor left, PaletteColor right)
    {
        return !(left == right);
    }

    public static PaletteColor Parse(string hexString)
    {
        string hex = hexString.Replace("#", string.Empty);

        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        if (hex.Length != 6)
        {
            throw new ArgumentException("Invalid hex string. Expected format: RRGGBB");
        }

        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);

        return new PaletteColor(r, g, b);
    }
}
