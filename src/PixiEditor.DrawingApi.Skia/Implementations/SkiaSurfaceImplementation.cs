using System;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private readonly SkiaCanvasImplementation _canvasImplementation;
        private readonly SkiaPaintImplementation _paintImplementation;

        public GRContext GrContext { get; set; }

        public SkiaSurfaceImplementation(GRContext context, SkiaPixmapImplementation pixmapImplementation, SkiaCanvasImplementation canvasImplementation, SkiaPaintImplementation paintImplementation)
        {
            _pixmapImplementation = pixmapImplementation;
            _canvasImplementation = canvasImplementation;
            _paintImplementation = paintImplementation;
            GrContext = context;
        }
        
        public Pixmap PeekPixels(DrawingSurface drawingSurface)
        {
            SKPixmap pixmap = ManagedInstances[drawingSurface.ObjectPointer].PeekPixels();
            if (pixmap == null)
            {
                return drawingSurface.Snapshot().PeekPixels();
            }
            
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX,
            int srcY)
        {
            return ManagedInstances[drawingSurface.ObjectPointer]
                .ReadPixels(dstInfo.ToSkImageInfo(), dstPixels, dstRowBytes, srcX, srcY);
        }

        public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
        {
            SKCanvas canvas = _canvasImplementation[surfaceToDraw.ObjectPointer];
            SKPaint paint = _paintImplementation[drawingPaint.ObjectPointer];
            var instance = ManagedInstances[drawingSurface.ObjectPointer];
            instance.Draw(canvas, x, y, paint);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            SKSurface skSurface = CreateSkiaSurface(imageInfo.Size, !imageInfo.ForceCpu);
            
            var canvas = skSurface.Canvas;
            canvas.DrawImage(SKImage.FromPixelCopy(imageInfo.ToSkImageInfo(), pixels, rowBytes), new SKPoint(0, 0));
            
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKSurface skSurface = CreateSkiaSurface(imageInfo.Size, !imageInfo.ForceCpu);
            
            var canvas = skSurface.Canvas;
            canvas.DrawImage(SKImage.FromPixelCopy(imageInfo.ToSkImageInfo(), pixelBuffer), new SKPoint(0, 0));
            
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(Pixmap pixmap)
        {
            SKPixmap skPixmap = _pixmapImplementation[pixmap.ObjectPointer];
            SKImageInfo info = skPixmap.Info;
            SKSurface skSurface = CreateSkiaSurface(new VecI(info.Width, info.Height), true);
            
            var canvas = skSurface.Canvas;
            canvas.DrawImage(SKImage.FromPixels(skPixmap), new SKPoint(0, 0));
            
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(ImageInfo imageInfo)
        {
            SKSurface skSurface = CreateSkiaSurface(imageInfo.Size, !imageInfo.ForceCpu);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface CreateSkiaSurface(VecI size, bool gpu)
        {
            if (!gpu)
            {
                return SKSurface.Create(new SKImageInfo(size.X, size.Y));
            }
            
            return SKSurface.Create(GrContext, false, new SKImageInfo(size.X, size.Y));
        }

        public void Dispose(DrawingSurface drawingSurface)
        {
            ManagedInstances[drawingSurface.ObjectPointer].Dispose();
            ManagedInstances.TryRemove(drawingSurface.ObjectPointer, out _);
        }

        public object GetNativeSurface(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }

        private DrawingSurface CreateDrawingSurface(SKSurface skSurface)
        {
            _canvasImplementation.ManagedInstances[skSurface.Canvas.Handle] = skSurface.Canvas;
            Canvas canvas = new Canvas(skSurface.Canvas.Handle);

            DrawingSurface surface = new DrawingSurface(skSurface.Handle, canvas);
            ManagedInstances[skSurface.Handle] = skSurface;
            
            return surface;
        }
        
        public void Flush(DrawingSurface drawingSurface)
        {
            ManagedInstances[drawingSurface.ObjectPointer].Flush(true);
        }
    }
}
