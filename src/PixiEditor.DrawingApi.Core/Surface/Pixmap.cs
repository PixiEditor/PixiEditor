using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Surface;

public class Pixmap : NativeObject
{
    internal Pixmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public static Pixmap InternalCreateFromExistingPointer(IntPtr objPointer)
    {
        return new Pixmap(objPointer);
    }
    
    public Pixmap(ImageInfo imgInfo, IntPtr dataPtr) : base(dataPtr)
    {
        ObjectPointer = DrawingBackendApi.Current.PixmapImplementation.Construct(dataPtr, imgInfo);
    }

    public int Width
    {
        get => DrawingBackendApi.Current.PixmapImplementation.GetWidth(this);
    }

    public int Height
    {
        get => DrawingBackendApi.Current.PixmapImplementation.GetHeight(this);
    }

    public int BytesSize => DrawingBackendApi.Current.PixmapImplementation.GetBytesSize(this);

    public override void Dispose()
    {
        DrawingBackendApi.Current.PixmapImplementation.Dispose(ObjectPointer);
    }

    public IntPtr GetPixels()
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixels(ObjectPointer);
    }

    public Span<T> GetPixelSpan<T>() where T : unmanaged
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixelSpan<T>(this);
    }
}
