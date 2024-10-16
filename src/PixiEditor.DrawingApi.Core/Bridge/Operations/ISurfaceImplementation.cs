using System;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations;

public interface ISurfaceImplementation
{
    public Pixmap PeekPixels(DrawingSurface drawingSurface);
    public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes);
    public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
    public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint);
    public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer);
    public DrawingSurface Create(Pixmap pixmap);
    public DrawingSurface Create(ImageInfo imageInfo);
    public void Dispose(DrawingSurface drawingSurface);
    public object GetNativeSurface(IntPtr objectPointer);
    public void Flush(DrawingSurface drawingSurface);
    public DrawingSurface FromNative(object native);
    public RectI GetDeviceClipBounds(DrawingSurface surface);
    public RectD GetLocalClipBounds(DrawingSurface surface);
    public void Unmanage(DrawingSurface surface);
}

