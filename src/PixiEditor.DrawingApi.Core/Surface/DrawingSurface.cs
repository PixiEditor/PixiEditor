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
        
        internal DrawingSurface(IntPtr objPtr) : base(objPtr)
        {
        }
        
        public static DrawingSurface Create(Pixmap imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceOperations.Create(imageInfo);
        }
        
        public void Draw(Canvas drawingSurfaceCanvas, int x, int y, Paint drawingPaint)
        {
            DrawingBackendApi.Current.SurfaceOperations.Draw(this, drawingSurfaceCanvas, x, y, drawingPaint);
        }

        public Image Snapshot()
        {
            return DrawingBackendApi.Current.ImageOperations.Snapshot(this);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.SurfaceOperations.PeekPixels(this);
        }
        
        public bool ReadPixels(ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
        {
            return DrawingBackendApi.Current.SurfaceOperations.ReadPixels(this, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceOperations.Create(imageInfo);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            return DrawingBackendApi.Current.SurfaceOperations.Create(imageInfo, pixels, rowBytes);
        }
        
        public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            return DrawingBackendApi.Current.SurfaceOperations.Create(imageInfo, pixelBuffer);
        }

        public override void Dispose()
        {
        }
    }
}
