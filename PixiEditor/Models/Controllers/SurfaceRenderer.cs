using PixiEditor.Models.DataHolders;
using SkiaSharp;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    class SurfaceRenderer : IDisposable
    {
        public SKSurface BackingSurface { get; private set; }
        public WriteableBitmap FinalBitmap { get; private set; }
        private SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        public SurfaceRenderer(int width, int height)
        {
            FinalBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            var imageInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            BackingSurface = SKSurface.Create(imageInfo, FinalBitmap.BackBuffer, FinalBitmap.BackBufferStride);
        }

        public void Dispose()
        {
            BackingSurface.Dispose();
            BlendingPaint.Dispose();
        }

        public void Draw(Surface otherSurface, byte opacity)
        {
            BackingSurface.Canvas.Clear();
            FinalBitmap.Lock();
            BlendingPaint.Color = new SKColor(255, 255, 255, opacity);
            //otherSurface.SkiaSurface.Draw(BackingSurface.Canvas, 0, 0, BlendingPaint);
            BackingSurface.Canvas.DrawImage(otherSurface.SkiaSurface.Snapshot(), new SKRect(0, 0, FinalBitmap.PixelWidth, FinalBitmap.PixelHeight));
            FinalBitmap.AddDirtyRect(new Int32Rect(0, 0, FinalBitmap.PixelWidth, FinalBitmap.PixelHeight));
            FinalBitmap.Unlock();
        }
    }
}
