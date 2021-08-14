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
        private static readonly SKPaint nearestNeighborReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.Low };

        public SKSurface SkiaSurface { get; }
        public int Width { get; }
        public int Height { get; }

        private SKPaint drawingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };

        public Surface(int w, int h)
        {
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
            SKImageInfo info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
            var ptr = Marshal.AllocHGlobal(pbgra32Bytes.Length);
            try
            {
                Marshal.Copy(pbgra32Bytes, 0, ptr, pbgra32Bytes.Length);
                SKPixmap map = new(info, ptr);
                SKSurface surface = SKSurface.Create(map);
                var newSurface = CreateSurface(w, h);
                surface.Draw(newSurface.Canvas, 0, 0, ReplacingPaint);
                SkiaSurface = newSurface;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public Surface ResizeNearestNeighbor(int newW, int newH)
        {
            SKImage image = SkiaSurface.Snapshot();
            Surface newSurface = new(newW, newH);
            newSurface.SkiaSurface.Canvas.DrawImage(image, new SKRect(0, 0, newW, newH), nearestNeighborReplacingPaint);
            return newSurface;
        }

        /// <summary>
        /// probably doesn't work correctly
        /// </summary>
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

        /// <summary>
        /// probably doesn't work correctly
        /// </summary>
        public byte[] ToPbgra32ByteArray()
        {
            return SkiaSurface.Snapshot().Encode(SKEncodedImageFormat.Bmp, 100).ToArray();
        }

        public WriteableBitmap ToWriteableBitmap()
        {
            WriteableBitmap result = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            result.Lock();
            result.CopyPixels(ToPbgra32ByteArray(), Width * 4, 0);
            result.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            result.Unlock();
            return result;
        }

        public void Dispose()
        {
            SkiaSurface.Dispose();
        }

        private static SKSurface CreateSurface(int w, int h)
        {
            return SKSurface.Create(new SKImageInfo(0, 0, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        }

    }
}
