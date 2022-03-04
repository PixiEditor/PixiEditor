using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChunkyImageLib
{
    public class ImageData : IDisposable
    {
        private bool disposed;
        private int bytesPerPixel;
        public SKColorType ColorType { get; }
        public IntPtr PixelBuffer { get; }
        public SKSurface SkiaSurface { get; }
        public int Width { get; }
        public int Height { get; }
        public ImageData(int width, int height, SKColorType colorType)
        {
            if (colorType is not SKColorType.RgbaF16 or SKColorType.Bgra8888)
                throw new ArgumentException("Unsupported color type");
            if (width < 1 || height < 1)
                throw new ArgumentException("Width and height must be >1");
            if (width > 10000 || height > 1000)
                throw new ArgumentException("Width and height must be <=10000");

            ColorType = colorType;
            bytesPerPixel = colorType == SKColorType.RgbaF16 ? 8 : 4;
            PixelBuffer = CreateBuffer(width, height, bytesPerPixel);
            SkiaSurface = CreateSKSurface();
        }

        public unsafe void CopyTo(ImageData other)
        {
            if (other.Width != Width || other.Height != Height || other.ColorType != ColorType)
                throw new ArgumentException("Target ImageData must have the same format");
            int bytesC = Width * Height * bytesPerPixel;
            Buffer.MemoryCopy((void*)PixelBuffer, (void*)other.PixelBuffer, bytesC, bytesC);
        }

        public unsafe SKColor GetSRGBPixel(int x, int y)
        {
            if (ColorType == SKColorType.RgbaF16)
            {
                Half* ptr = (Half*)(PixelBuffer + (x + y * Width) * bytesPerPixel);
                float a = (float)ptr[3];
                return (SKColor)new SKColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);
            }
            else
            {
                // todo later
                throw new NotImplementedException();
            }
        }

        private SKSurface CreateSKSurface()
        {
            var surface = SKSurface.Create(new SKImageInfo(Width, Height, ColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()), PixelBuffer);
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

        ~ImageData()
        {
            Marshal.FreeHGlobal(PixelBuffer);
        }
    }
}
