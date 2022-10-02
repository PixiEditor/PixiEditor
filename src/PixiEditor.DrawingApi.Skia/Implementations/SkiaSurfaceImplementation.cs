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
            throw new NotImplementedException();
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX,
            int srcY)
        {
            throw new NotImplementedException();
        }

        public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
        {
            throw new NotImplementedException();
        }

        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            throw new NotImplementedException();
        }

        public DrawingSurface Create(Pixmap pixmap)
        {
            throw new NotImplementedException();
        }

        public DrawingSurface Create(ImageInfo imageInfo)
        {
            throw new NotImplementedException();
        }
    }
}
