using System;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private readonly SkiaCanvasImplementation _canvasImplementation;
        private readonly SkiaPaintImplementation _paintImplementation;

        private GRContext? _grContext;

        public SkiaSurfaceImplementation(SkiaPixmapImplementation pixmapImplementation, SkiaCanvasImplementation canvasImplementation, SkiaPaintImplementation paintImplementation)
        {
            _pixmapImplementation = pixmapImplementation;
            _canvasImplementation = canvasImplementation;
            _paintImplementation = paintImplementation;
        }
        
        public Pixmap PeekPixels(DrawingSurface drawingSurface)
        {
            SKPixmap pixmap = ManagedInstances[drawingSurface.ObjectPointer].PeekPixels();
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
            ManagedInstances[drawingSurface.ObjectPointer].Draw(canvas, x, y, paint);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            SKSurface skSurface;
            if (_grContext != null)
            {
                skSurface = CreateGrContextSurface(imageInfo.ToSkImageInfo());
            }
            else
            {
                skSurface = SKSurface.Create(imageInfo.ToSkImageInfo(), pixels, rowBytes);
            }

            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKSurface skSurface;
            if (_grContext != null)
            {
                skSurface = CreateGrContextSurface(imageInfo.ToSkImageInfo(), pixelBuffer);
            }
            else
            {
                skSurface = SKSurface.Create(imageInfo.ToSkImageInfo(), pixelBuffer);
            }
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(Pixmap pixmap)
        {
            SKPixmap skPixmap = _pixmapImplementation[pixmap.ObjectPointer];
            SKSurface skSurface;
            if (_grContext != null)
            {
                // TODO: This is not correct lol, leaving for debugging purposes right now
                skSurface = CreateGrContextSurface(skPixmap.Info);
            }
            else
            {
                skSurface = SKSurface.Create(skPixmap);
            }

            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface Create(ImageInfo imageInfo)
        {
            SKSurface skSurface;
            if (_grContext != null)
            {
                skSurface = CreateGrContextSurface(imageInfo.ToSkImageInfo());
            }
            else
            {
                skSurface = SKSurface.Create(imageInfo.ToSkImageInfo());
            }

            return CreateDrawingSurface(skSurface);
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

        private SKSurface CreateGrContextSurface(SKImageInfo info)
        {
            GRGlFramebufferInfo framebuffer = new GRGlFramebufferInfo((uint)0, info.ColorType.ToGlSizedFormat());
            GRBackendRenderTarget renderTarget = new GRBackendRenderTarget(info.Width, info.Height, 4, 0, framebuffer);
            return SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKImageInfo.PlatformColorType);
        }

        private SKSurface CreateGrContextSurface(SKImageInfo info, IntPtr pixelBuffer)
        {
            GRGlFramebufferInfo framebuffer = new GRGlFramebufferInfo((uint)0, info.ColorType.ToGlSizedFormat());
            GRBackendRenderTarget renderTarget = new GRBackendRenderTarget(info.Width, info.Height, 4, 0, framebuffer);
            return SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, info.ColorType, info.ColorSpace);
        }

        private DrawingSurface CreateDrawingSurface(SKSurface skSurface)
        {
            _canvasImplementation.ManagedInstances[skSurface.Canvas.Handle] = skSurface.Canvas;
            Canvas canvas = new Canvas(skSurface.Canvas.Handle);

            DrawingSurface surface = new DrawingSurface(skSurface.Handle, canvas);
            ManagedInstances[skSurface.Handle] = skSurface;
            return surface;
        }

        public void SetGrContext(GRContext grContext)
        {
            _grContext = grContext;
        }
    }
}
