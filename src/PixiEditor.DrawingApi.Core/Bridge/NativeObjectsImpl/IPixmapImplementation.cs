using System;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IPixmapImplementation
{
    public void Dispose(IntPtr objectPointer);

    public Color GetPixelColor(IntPtr objectPointer, VecI position);
    
    public IntPtr GetPixels(IntPtr objectPointer);

    public Span<T> GetPixelSpan<T>(Pixmap pixmap)
        where T : unmanaged;

    public IntPtr Construct(IntPtr dataPtr, ImageInfo imgInfo);

    public int GetWidth(Pixmap pixmap);

    public int GetHeight(Pixmap pixmap);

    public int GetBytesSize(Pixmap pixmap);
    public object GetNativePixmap(IntPtr objectPointer);
    public Color GetColor(Pixmap pixmap, int x, int y);
}
