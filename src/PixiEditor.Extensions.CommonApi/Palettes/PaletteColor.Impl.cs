namespace PixiEditor.Extensions.CommonApi.Palettes;

public partial class PaletteColor : IEquatable<PaletteColor>
{
    public static PaletteColor Empty => new PaletteColor(0, 0, 0);
    public static PaletteColor Black => new PaletteColor(0, 0, 0);
    public static PaletteColor White => new PaletteColor(255, 255, 255);

    public byte R
    {
        get => (byte)RValue;
        set => RValue = value;
    }

    public byte G
    {
        get => (byte)GValue;
        set => GValue = value;
    }

    public byte B
    {
        get => (byte)BValue;
        set => BValue = value;
    }

    public string Hex => $"#{R:X2}{G:X2}{B:X2}";

    public PaletteColor(byte r, byte g, byte b)
    {
        RValue = r;
        GValue = g;
        BValue = b;
    }

    public PaletteColor(uint r, uint g, uint b)
    {
        RValue = (byte)r;
        GValue = (byte)g;
        BValue = (byte)b;
    }

    public PaletteColor()
    {
    }

    public override string ToString()
    {
        return Hex;
    }

    public static bool operator ==(PaletteColor left, PaletteColor right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

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

    public bool Equals(PaletteColor other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return RValue == other.RValue && GValue == other.GValue && BValue == other.BValue;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((PaletteColor)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RValue, GValue, BValue);
    }
}
