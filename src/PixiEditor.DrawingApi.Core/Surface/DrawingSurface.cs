using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class DrawingSurface : NativeObject, IPixelsMap
    {
        public override object Native => DrawingBackendApi.Current.SurfaceImplementation.GetNativeSurface(ObjectPointer);
        public Canvas Canvas { get; private set; }
        public event SurfaceChangedEventHandler? Changed;

        public DrawingSurface(IntPtr objPtr, Canvas canvas) : base(objPtr)
        {
            Canvas = canvas;
            Canvas.Changed += OnCanvasChanged;
        }

        public static DrawingSurface Create(Pixmap imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }
        
        public void Draw(Canvas drawingSurfaceCanvas, int x, int y, Paint drawingPaint)
        {
            DrawingBackendApi.Current.SurfaceImplementation.Draw(this, drawingSurfaceCanvas, x, y, drawingPaint);
            Changed?.Invoke(null);
        }

        public Image Snapshot()
        {
            return DrawingBackendApi.Current.ImageImplementation.Snapshot(this);
        }

        public Image Snapshot(RectI bounds)
        {
            return DrawingBackendApi.Current.ImageImplementation.Snapshot(this, bounds);
        }

        public Pixmap PeekPixels()
        {
            return DrawingBackendApi.Current.SurfaceImplementation.PeekPixels(this);
        }
        
        public bool ReadPixels(ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.ReadPixels(this, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
        }
        
        public static DrawingSurface Create(ImageInfo imageInfo)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo);
        }

        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixels, rowBytes);
        }
        
        public static DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            return DrawingBackendApi.Current.SurfaceImplementation.Create(imageInfo, pixelBuffer);
        }

        public override void Dispose()
        {
            Canvas.Changed -= OnCanvasChanged;
            Canvas.Dispose(); // TODO: make sure this is correct
            DrawingBackendApi.Current.SurfaceImplementation.Dispose(this);
        }

        private void OnCanvasChanged(RectD? changedrect)
        {
            Changed?.Invoke(changedrect);
        }
    }
}
