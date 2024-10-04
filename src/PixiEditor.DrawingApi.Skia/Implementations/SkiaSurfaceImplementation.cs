using System;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private readonly SkiaCanvasImplementation _canvasImplementation;
        private readonly SkiaPaintImplementation _paintImplementation;

        internal GRContext? GrContext { get; set; }

        public SkiaSurfaceImplementation(GRContext context, SkiaPixmapImplementation pixmapImplementation,
            SkiaCanvasImplementation canvasImplementation, SkiaPaintImplementation paintImplementation)
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
                using var snapshot = drawingSurface.Snapshot();
                Bitmap bitmap = Bitmap.FromImage(snapshot);
                return bitmap.PeekPixels();
            }

            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes,
            int srcX,
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
            SKSurface skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked, pixels, rowBytes);
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKImageInfo info = imageInfo.ToSkImageInfo();
            SKSurface skSurface = CreateSkiaSurface(info, imageInfo.GpuBacked, pixelBuffer);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels, int rowBytes)
        {
            if (isGpuBacked)
            {
                SKSurface skSurface = CreateSkiaSurface(imageInfo, true);
                using var image = SKImage.FromPixelCopy(imageInfo, pixels, rowBytes);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels, rowBytes);
        }

        private SKSurface CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels)
        {
            if (isGpuBacked)
            {
                SKSurface skSurface = CreateSkiaSurface(imageInfo, true);
                using var image = SKImage.FromPixelCopy(imageInfo, pixels);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels);
        }

        public DrawingSurface Create(Pixmap pixmap)
        {
            SKPixmap skPixmap = _pixmapImplementation[pixmap.ObjectPointer];
            var skSurface = CreateSkiaSurface(skPixmap);

            return CreateDrawingSurface(skSurface);
        }

        private SKSurface CreateSkiaSurface(SKPixmap skPixmap)
        {
            SKSurface skSurface = SKSurface.Create(skPixmap); 
            return skSurface;
        }

        public DrawingSurface Create(ImageInfo imageInfo)
        {
            SKSurface skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface CreateSkiaSurface(SKImageInfo info, bool gpu)
        {
            if (!gpu || GrContext == null)
            {
                return SKSurface.Create(info);
            }

            return SKSurface.Create(GrContext, false, info);
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
            ManagedInstances[drawingSurface.ObjectPointer].Flush(true, true);
        }

        public DrawingSurface FromNative(object native)
        {
            if (native is not SKSurface skSurface)
            {
                throw new ArgumentException("Native object is not of type SKSurface");
            }

            return CreateDrawingSurface(skSurface);
        }

        public RectI GetDeviceClipBounds(DrawingSurface surface)
        {
            SKRectI rect = ManagedInstances[surface.ObjectPointer].Canvas.DeviceClipBounds;
            return new RectI(rect.Left, rect.Top, rect.Width, rect.Height);
        }
    }
}
