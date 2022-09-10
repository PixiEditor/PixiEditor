using System;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations;

public interface ISurfaceOperations
{
    public Pixmap PeekPixels(DrawingSurface drawingSurface);
    public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes);
    public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
    public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint);
}
