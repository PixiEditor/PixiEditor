using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public struct Edges
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
}
