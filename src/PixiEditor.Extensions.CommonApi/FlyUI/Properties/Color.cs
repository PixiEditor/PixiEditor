namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public struct Color : IStructProperty
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color FromRgba(byte r, byte g, byte b, byte a)
    {
        return new Color(r, g, b, a);
    }

    byte[] IStructProperty.Serialize()
    {
        return [R, G, B, A];
    }

    void IStructProperty.Deserialize(byte[] data)
    {
        R = data[0];
        G = data[1];
        B = data[2];
        A = data[3];
    }

    public static Color FromBytes(byte[] data)
    {
        if (data.Length < 4)
        {
            throw new ArgumentException("Data array must contain at least 4 bytes.");
        }


        return new Color(data[0], data[1], data[2], data[3]);
    }
}
