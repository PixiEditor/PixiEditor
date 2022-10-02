using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class DrawingSurface : NativeObject
    {
        public float Width { get; set; }
        public float Height { get; set; }
        
        public DrawingSurfaceProperties Properties { get; private set; }
        public Canvas Canvas { get; private set; }
        
        public DrawingSurface(IntPtr objPtr) : base(objPtr)
        {
        }
        
        public static DrawingSurface Create(Pixmap imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }
        
        public void Draw(Canvas drawingSurfaceCanvas, int x, int y, Paint drawingPaint)
        {
            DrawingBackendApi.Current.SurfaceImplementation.Draw(this, drawingSurfaceCanvas, x, y, drawingPaint);
        }

        public Image Snapshot()
        {
            return DrawingBackendApi.Current.ImageImplementation.Snapshot(this);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.SurfaceImplementation.PeekPixels(this);
        }
        
        public bool ReadPixels(ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.ReadPixels(this, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixels, rowBytes);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixelBuffer);
        }

        public override void Dispose()
        {
        }
    }
}
