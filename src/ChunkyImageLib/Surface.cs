using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public class Surface : IDisposable
{
    private bool disposed;
    private int bytesPerPixel;
    public IntPtr PixelBuffer { get; }
    public SKSurface SkiaSurface { get; }
    public VecI Size { get; }

    public Surface(VecI size)
    {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentException("Width and height must be >1");
        if (size.X > 10000 || size.Y > 10000)
            throw new ArgumentException("Width and height must be <=10000");

        Size = size;

        bytesPerPixel = 8;
        PixelBuffer = CreateBuffer(size.X, size.Y, bytesPerPixel);
        SkiaSurface = CreateSKSurface();
    }

    public Surface(Surface original) : this(original.Size)
    {
        SkiaSurface.Canvas.DrawSurface(original.SkiaSurface, 0, 0);
    }

    public static Surface Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);
        using var bitmap = SKBitmap.Decode(path);
        if (bitmap is null)
            throw new ArgumentException($"The image with path {path} couldn't be loaded");
        var surface = new Surface(new VecI(bitmap.Width, bitmap.Height));
        surface.SkiaSurface.Canvas.DrawBitmap(bitmap, 0, 0);
        return surface;
    }

    public unsafe void CopyTo(Surface other)
    {
        if (other.Size != Size)
            throw new ArgumentException("Target Surface must have the same dimensions");
        int bytesC = Size.X * Size.Y * bytesPerPixel;
        Buffer.MemoryCopy((void*)PixelBuffer, (void*)other.PixelBuffer, bytesC, bytesC);
    }

    public unsafe SKColor GetSRGBPixel(int x, int y)
    {
        throw new NotImplementedException("This function needs to be changed to account for linear -> srgb conversion");
        /*
        Half* ptr = (Half*)(PixelBuffer + (x + y * Size.X) * bytesPerPixel);
        float a = (float)ptr[3];
        return (SKColor)new SKColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);*/
    }

    public unsafe bool IsFullyTransparent()
    {
        ulong* ptr = (ulong*)PixelBuffer;
        for (int i = 0; i < Size.X * Size.Y; i++)
        {
            // ptr[i] actually contains 4 16-bit floats. We only care about the first one which is alpha.
            // An empty pixel can have alpha of 0 or -0 (not sure if -0 actually ever comes up). 0 in hex is 0x0, -0 in hex is 0x8000
            if ((ptr[i] & 0x1111_0000_0000_0000) != 0 && (ptr[i] & 0x1111_0000_0000_0000) != 0x8000_0000_0000_0000)
                return false;
        }
        return true;
    }

    public void SaveToDesktop(string filename = "savedSurface.png")
    {
        using var final = SKSurface.Create(new SKImageInfo(Size.X, Size.Y, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        final.Canvas.DrawSurface(SkiaSurface, 0, 0);
        using (var snapshot = final.Snapshot())
        {
            using var stream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), filename));
            using var png = snapshot.Encode();
            png.SaveTo(stream);
        }
    }

    private SKSurface CreateSKSurface()
    {
        var surface = SKSurface.Create(new SKImageInfo(Size.X, Size.Y, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear()), PixelBuffer);
        if (surface is null)
            throw new InvalidOperationException($"Could not create surface (Size:{Size})");
        return surface;
    }

    private unsafe static IntPtr CreateBuffer(int width, int height, int bytesPerPixel)
    {
        int byteC = width * height * bytesPerPixel;
        var buffer = Marshal.AllocHGlobal(byteC);
        Unsafe.InitBlockUnaligned((byte*)buffer, 0, (uint)byteC);
        return buffer;
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        Marshal.FreeHGlobal(PixelBuffer);
        GC.SuppressFinalize(this);
    }

    ~Surface()
    {
        Marshal.FreeHGlobal(PixelBuffer);
    }
}
