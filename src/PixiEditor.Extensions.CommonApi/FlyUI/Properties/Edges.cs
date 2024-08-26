using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public struct Edges : IStructProperty
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }

    public Edges(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
    
    public static Edges All(double value)
    {
        return new Edges(value, value, value, value);
    }
    
    public static Edges Symmetric(double vertical, double horizontal)
    {
        return new Edges(horizontal, vertical, horizontal, vertical);
    }
    
    public static Edges operator +(Edges a, Edges b)
    {
        return new Edges(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
    }
    
    public static Edges operator -(Edges a, Edges b)
    {
        return new Edges(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
    }
    
    public static Edges operator *(Edges a, double b)
    {
        return new Edges(a.Left * b, a.Top * b, a.Right * b, a.Bottom * b);
    }
    
    public static Edges operator /(Edges a, double b)
    {
        return new Edges(a.Left / b, a.Top / b, a.Right / b, a.Bottom / b);
    }

    byte[] IStructProperty.Serialize()
    {
        byte[] data = new byte[32];
        
        BitConverter.GetBytes(Left).CopyTo(data, 0);
        BitConverter.GetBytes(Top).CopyTo(data, 8);
        BitConverter.GetBytes(Right).CopyTo(data, 16);
        BitConverter.GetBytes(Bottom).CopyTo(data, 24);
        
        return data;
    }

    void IStructProperty.Deserialize(byte[] data)
    {
        Left = BitConverter.ToDouble(data, 0);
        Top = BitConverter.ToDouble(data, 8);
        Right = BitConverter.ToDouble(data, 16);
        Bottom = BitConverter.ToDouble(data, 24);
    }
}
