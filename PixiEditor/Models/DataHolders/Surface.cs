using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.DataHolders
{
    public class Surface : IDisposable
    {
        public static SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };

        private static readonly SKPaint nearestNeighborReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.None };

        public SKSurface SkiaSurface { get; }
        public int Width { get; }
        public int Height { get; }

        private SKPaint drawingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };

        public Surface(int w, int h)
        {
            if (w <= 0 || h <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");
            SkiaSurface = CreateSurface(w, h);
            Width = w;
            Height = h;
        }

        public Surface(Surface original)
        {
            Width = original.Width;
            Height = original.Height;
            var newSurface = CreateSurface(Width, Height);
            original.SkiaSurface.Draw(newSurface.Canvas, 0, 0, ReplacingPaint);
            SkiaSurface = newSurface;
        }

        public Surface(int w, int h, byte[] pbgra32Bytes)
        {
            if (w <= 0 || h <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");
            Width = w;
            Height = h;
            SkiaSurface = Pbgra32BytesToSkSurface(w, h, pbgra32Bytes);
        }

        public Surface(BitmapSource original)
        {
            if (original.Format != PixelFormats.Pbgra32)
                throw new ArgumentException("This method only supports Pbgra32 bitmaps");
            if (original.PixelWidth <= 0 || original.PixelHeight <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");

            byte[] pixels = new byte[original.PixelWidth * original.PixelHeight * 4];
            original.CopyPixels(pixels, original.PixelWidth * 4, 0);

            Width = original.PixelWidth;
            Height = original.PixelHeight;
            SkiaSurface = Pbgra32BytesToSkSurface(Width, Height, pixels);
        }

        public Surface(SKImage image)
        {
            Width = image.Width;
            Height = image.Height;
            SkiaSurface = CreateSurface(Width, Height);
            SkiaSurface.Canvas.DrawImage(image, 0, 0);
        }

        public Surface ResizeNearestNeighbor(int newW, int newH)
        {
            SKImage image = SkiaSurface.Snapshot();
            Surface newSurface = new(newW, newH);
            newSurface.SkiaSurface.Canvas.DrawImage(image, new SKRect(0, 0, newW, newH), nearestNeighborReplacingPaint);
            return newSurface;
        }

        public Surface Crop(int x, int y, int width, int height)
        {
            Surface result = new Surface(width, height);
            SkiaSurface.Draw(result.SkiaSurface.Canvas, x, y, ReplacingPaint);
            return result;
        }

        public SKColor GetSRGBPixel(int x, int y)
        {
            var imageInfo = new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKBitmap bitmap = new SKBitmap(imageInfo);
            IntPtr dstpixels = bitmap.GetPixels();
            SkiaSurface.ReadPixels(imageInfo, dstpixels, imageInfo.RowBytes, x, y);
            return bitmap.GetPixel(0, 0);
        }

        public void SetSRGBPixel(int x, int y, SKColor color)
        {
            drawingPaint.Color = color;
            SkiaSurface.Canvas.DrawPoint(x, y, drawingPaint);
        }

        public byte[] ToPbgra32ByteArray()
        {
            var imageInfo = new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            var buffer = Marshal.AllocHGlobal(Width * Height * 4);
            try
            {
                using SKSurface surface = SKSurface.Create(imageInfo, buffer, Width * 4);
                SkiaSurface.Draw(surface.Canvas, 0, 0, ReplacingPaint);
                byte[] managed = new byte[Width * Height * 4];
                Marshal.Copy(buffer, managed, 0, Width * Height * 4);
                return managed;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public WriteableBitmap ToWriteableBitmap()
        {
            WriteableBitmap result = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            result.Lock();
            var dirty = new Int32Rect(0, 0, Width, Height);
            result.WritePixels(dirty, ToPbgra32ByteArray(), Width * 4, 0);
            result.AddDirtyRect(dirty);
            result.Unlock();
            return result;
        }

        public void Dispose()
        {
            SkiaSurface.Dispose();
        }

        private static unsafe SKSurface Pbgra32BytesToSkSurface(int w, int h, byte[] pbgra32Bytes)
        {
            SKImageInfo info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);

            fixed (void* pointer = pbgra32Bytes)
            {
                using SKPixmap map = new(info, new IntPtr(pointer));
                using SKSurface surface = SKSurface.Create(map);
                var newSurface = CreateSurface(w, h);
                surface.Draw(newSurface.Canvas, 0, 0, ReplacingPaint);
                return newSurface;
            }
        }

        private static SKSurface CreateSurface(int w, int h)
        {
            var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
            if (surface == null)
                throw new Exception("Could not create surface");
            return surface;
        }

    }
}
