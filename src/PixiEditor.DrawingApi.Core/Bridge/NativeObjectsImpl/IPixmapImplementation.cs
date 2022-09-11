using System;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IPixmapImplementation
{
    public void Dispose(IntPtr objectPointer);
    public IntPtr GetPixels(IntPtr objectPointer);
    public Span<T> GetPixelSpan<T>(Pixmap pixmap);
    public IntPtr Construct(IntPtr dataPtr, ImageInfo imgInfo);
}
