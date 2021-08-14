using SkiaSharp;
using System;

namespace PixiEditor.Models.DataHolders
{
    public class Surface
    {
        public static SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static readonly SKPaint nearestNeighborReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.Low };

        public SKSurface SKSurface { get; }
        public int Width { get; }
        public int Height { get; }

        private SKPaint drawingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };

        public Surface(int w, int h)
        {
            SKSurface = CreateSurface(w, h);
            Width = w;
            Height = h;
        }

        public Surface(Surface original)
        {
            Width = original.Width;
            Height = original.Height;
            var newSurface = CreateSurface(Width, Height);
            original.SKSurface.Draw(newSurface.Canvas, 0, 0, ReplacingPaint);
            SKSurface = newSurface;
        }

        public Surface ResizeNearestNeighbor(int newW, int newH)
        {
            SKImage image = SKSurface.Snapshot();
            Surface newSurface = new(newW, newH);
            newSurface.SKSurface.Canvas.DrawImage(image, new SKRect(0, 0, newW, newH), nearestNeighborReplacingPaint);
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
            SKSurface.ReadPixels(imageInfo, dstpixels, imageInfo.RowBytes, x, y);
            return bitmap.GetPixel(0, 0);
        }

        public void SetSRGBPixel(int x, int y, SKColor color)
        {
            drawingPaint.Color = color;
            SKSurface.Canvas.DrawPoint(x, y, drawingPaint);
        }

        /// <summary>
        /// probably doesn't work correctly
        /// </summary>
        public byte[] ToSRGBByteArray()
        {
            return SKSurface.Snapshot().Encode(SKEncodedImageFormat.Bmp, 100).ToArray();
        }

        private static SKSurface CreateSurface(int w, int h)
        {
            return SKSurface.Create(new SKImageInfo(0, 0, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        }
    }
}
