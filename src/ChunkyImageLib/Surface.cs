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
        public int Width { get; }
        public int Height { get; }

        public Surface(int width, int height)
        {
            if (width < 1 || height < 1)
                throw new ArgumentException("Width and height must be >1");
            if (width > 10000 || height > 10000)
                throw new ArgumentException("Width and height must be <=10000");

            Width = width;
            Height = height;

            bytesPerPixel = 8;
            PixelBuffer = CreateBuffer(width, height, bytesPerPixel);
            SkiaSurface = CreateSKSurface();
        }

        public Surface(Surface original) : this(original.Width, original.Height)
        {
            SkiaSurface.Canvas.DrawSurface(original.SkiaSurface, 0, 0);
        }

        public unsafe void CopyTo(Surface other)
        {
            if (other.Width != Width || other.Height != Height)
                throw new ArgumentException("Target Surface must have the same dimensions");
            int bytesC = Width * Height * bytesPerPixel;
            Buffer.MemoryCopy((void*)PixelBuffer, (void*)other.PixelBuffer, bytesC, bytesC);
        }

        public unsafe SKColor GetSRGBPixel(int x, int y)
        {
            Half* ptr = (Half*)(PixelBuffer + (x + y * Width) * bytesPerPixel);
            float a = (float)ptr[3];
            return (SKColor)new SKColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);
        }

        public void SaveToDesktop(string filename = "savedSurface.png")
        {
            using var final = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
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
            var surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear()), PixelBuffer);
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
