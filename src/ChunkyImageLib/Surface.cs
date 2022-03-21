using ChunkyImageLib.DataHolders;
using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChunkyImageLib
{
    public class Surface : IDisposable
    {
        private bool disposed;
        private int bytesPerPixel;
        public IntPtr PixelBuffer { get; }
        public SKSurface SkiaSurface { get; }
        public Vector2i Size { get; }

        public Surface(Vector2i size)
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

        public unsafe void CopyTo(Surface other)
        {
            if (other.Size != Size)
                throw new ArgumentException("Target Surface must have the same dimensions");
            int bytesC = Size.X * Size.Y * bytesPerPixel;
            Buffer.MemoryCopy((void*)PixelBuffer, (void*)other.PixelBuffer, bytesC, bytesC);
        }

        public unsafe SKColor GetSRGBPixel(int x, int y)
        {
            Half* ptr = (Half*)(PixelBuffer + (x + y * Size.X) * bytesPerPixel);
            float a = (float)ptr[3];
            return (SKColor)new SKColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);
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
            if (surface == null)
                throw new Exception("Could not create surface");
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
}
