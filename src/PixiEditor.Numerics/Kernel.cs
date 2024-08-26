using System.Runtime.InteropServices;

namespace PixiEditor.Numerics;

public class Kernel : ICloneable, IEquatable<Kernel>
{
    private KernelArray _buffer;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int RadiusX => Width / 2;
    
    public int RadiusY => Height / 2;

    public float this[int x, int y]
    {
        get => _buffer[x, y];
        set => _buffer[x, y] = value;
    }

    public float Sum => _buffer.Sum;

    public Kernel()
    {
        Width = 3;
        Height = 3;
        _buffer = new KernelArray(3, 3);
    }
    
    public Kernel(int width, int height)
    {
        if (width % 2 == 0)
            throw new ArgumentException($"{width} must be odd", nameof(width));
        
        Width = width;
        Height = height;
        _buffer = new KernelArray(width, height);
    }
    
    public Kernel(int width, int height, float[] values)
    {
        if (width % 2 == 0)
            throw new ArgumentException($"{width} must be odd", nameof(width));
        
        Width = width;
        Height = height;
        _buffer = new KernelArray(width, height, values);
    }

    public static Kernel Identity(int width, int height) =>
        new(width, height) { [0, 0] = 1 };

    public void Resize(int width, int height)
    {
        var old = _buffer;

        _buffer = new KernelArray(width, height);
        Width = width;
        Height = height;

        var oldRadiusX = old.RadiusX;
        var oldRadiusY = old.RadiusY;
        var newRadiusX = _buffer.RadiusX;
        var newRadiusY = _buffer.RadiusY;
        for (int y = -newRadiusY; y <= newRadiusY; y++)
        {
            for (int x = -newRadiusX; x <= newRadiusX; x++)
            {
                if (x < -oldRadiusX || x > oldRadiusX || y < -oldRadiusY || y > oldRadiusY)
                    continue;

                _buffer[x, y] = old[x, y];
            }
        }
    }

    public ReadOnlySpan<float> AsSpan() => _buffer.AsSpan();
    public object Clone()
    {
        float[] values = new float[Width * Height];
        AsSpan().CopyTo(values);
        return new Kernel(Width, Height, values);
    }

    public bool Equals(Kernel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Width == other.Width && Height == other.Height && _buffer.Equals(other._buffer);
    }
}
