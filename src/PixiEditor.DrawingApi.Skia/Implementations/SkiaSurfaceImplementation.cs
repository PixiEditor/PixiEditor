using System;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        
        public SkiaSurfaceImplementation(SkiaPixmapImplementation pixmapImplementation)
        {
            _pixmapImplementation = pixmapImplementation;
        }
        
        public Pixmap PeekPixels(DrawingSurface drawingSurface)
        {
            SKPixmap pixmap = ManagedInstances[drawingSurface.ObjectPointer].PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            SKSurface skSurface = SKSurface.Create(imageInfo.ToSkImageInfo(), pixels, rowBytes);
            DrawingSurface surface = new DrawingSurface(skSurface.Handle);
            ManagedInstances[skSurface.Handle] = skSurface;
            return surface;
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX,
            int srcY)
        {
            return ManagedInstances[drawingSurface.ObjectPointer]
                .ReadPixels(dstInfo.ToSkImageInfo(), dstPixels, dstRowBytes, srcX, srcY);
        }

        public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
        {
            ManagedInstances[drawingSurface.ObjectPointer].Draw(surfaceToDraw, x, y, drawingPaint);
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKSurface skSurface = SKSurface.Create(imageInfo.ToSkImageInfo(), pixelBuffer);
            DrawingSurface surface = new DrawingSurface(skSurface.Handle);
            ManagedInstances[skSurface.Handle] = skSurface;
            return surface;
        }

        public DrawingSurface Create(Pixmap pixmap)
        {
            SKSurface skSurface = SKSurface.Create(pixmap);
            DrawingSurface surface = new DrawingSurface(skSurface.Handle);
            ManagedInstances[skSurface.Handle] = skSurface;
        }

        public DrawingSurface Create(ImageInfo imageInfo)
        {
            SKSurface skSurface = SKSurface.Create(imageInfo.ToSkImageInfo());
            DrawingSurface surface = new DrawingSurface(skSurface.Handle);
            ManagedInstances[skSurface.Handle] = skSurface;
            return surface;
        }
    }
}
