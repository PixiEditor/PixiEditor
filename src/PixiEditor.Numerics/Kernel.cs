using System.Runtime.InteropServices;

namespace PixiEditor.Numerics;

public class Kernel : ISerializable
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
    public byte[] Serialize()
    {
        Span<byte> data = stackalloc byte[Width * Height * sizeof(float) + sizeof(int) * 2];
        BitConverter.GetBytes(Width).CopyTo(data);
        BitConverter.GetBytes(Height).CopyTo(data[sizeof(int)..]); 
        var span = AsSpan();
        MemoryMarshal.Cast<float, byte>(span).CopyTo(data);
        return data.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        if (data.Length < sizeof(int) * 2)
            throw new ArgumentException("Data is too short.", nameof(data));
        
        Width = BitConverter.ToInt32(data);
        Height = BitConverter.ToInt32(data[sizeof(int)..]);
        _buffer = new KernelArray(Width, Height, MemoryMarshal.Cast<byte, float>(data.AsSpan(sizeof(int) * 2)).ToArray());
    }
}
