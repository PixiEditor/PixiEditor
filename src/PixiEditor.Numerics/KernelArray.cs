namespace PixiEditor.Numerics;

public class KernelArray
{
    private readonly float[] _buffer;
    
    public int Width { get; }
    
    public int Height { get; }

    public int RadiusX => Width / 2;
    
    public int RadiusY => Height / 2;

    public float Sum => _buffer.Sum();
    
    public KernelArray(int width, int height)
    {
        if (width % 2 == 0)
            throw new ArgumentException($"{width} must be odd", nameof(width));
        
        Width = width;
        Height = height;
        _buffer = new float[width * height];
    }

    public float this[int x, int y]
    {
        get => _buffer[GetBufferIndex(x, y)];
        set => _buffer[GetBufferIndex(x, y)] = value;
    }

    private int GetBufferIndex(int x, int y)
    {
        x += RadiusX;
        y += RadiusY;

        return y * Width + x;
    }

    public ReadOnlySpan<float> AsSpan() => _buffer.AsSpan();
}
