using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces;

public class Pixmap : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PixmapImplementation.GetNativePixmap(ObjectPointer);

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

    public Color GetPixelColor(int x, int y) => GetPixelColor(new VecI(x, y));
    
    public Color GetPixelColor(VecI position)
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixelColor(ObjectPointer, position);
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
