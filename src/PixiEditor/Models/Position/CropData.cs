using System.IO;
using System.Runtime.InteropServices;

namespace PixiEditor.Models.Position;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct CropData
{
    [FieldOffset(0)]
    private readonly int _width;
    [FieldOffset(4)]
    private readonly int _height;
    [FieldOffset(8)]
    private readonly int _offsetX;
    [FieldOffset(12)]
    private readonly int _offsetY;

    public int Width => _width;

    public int Height => _height;

    public int OffsetX => _offsetX;

    public int OffsetY => _offsetY;

    public CropData(int width, int height, int offsetX, int offsetY)
    {
        _width = width;
        _height = height;
        _offsetX = offsetX;
        _offsetY = offsetY;
    }

    public static CropData FromByteArray(byte[] data)
    {
        if (data.Length != sizeof(CropData))
        {
            throw new ArgumentOutOfRangeException(nameof(data), $"data must be {sizeof(CropData)} long");
        }

        fixed (void* ptr = data)
        {
            return Marshal.PtrToStructure<CropData>(new IntPtr(ptr));
        }
    }

    public static CropData FromStream(Stream stream)
    {
        if (stream.Length < sizeof(CropData))
        {
            throw new ArgumentOutOfRangeException(nameof(stream), $"The specified stream must be at least {sizeof(CropData)} bytes long");
        }

        byte[] buffer = new byte[sizeof(CropData)];
        stream.Read(buffer);

        return FromByteArray(buffer);
    }

    public byte[] ToByteArray()
    {
        IntPtr ptr = Marshal.AllocHGlobal(sizeof(CropData));
        Marshal.StructureToPtr(this, ptr, true);

        Span<byte> bytes = new Span<byte>(ptr.ToPointer(), sizeof(CropData));
        byte[] array = bytes.ToArray();

        Marshal.FreeHGlobal(ptr);

        return array;
    }

    public MemoryStream ToStream()
    {
        MemoryStream stream = new();
        stream.Write(ToByteArray());
        return stream;
    }

}
