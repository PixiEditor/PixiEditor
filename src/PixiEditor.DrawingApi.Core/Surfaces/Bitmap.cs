using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces;

public class Bitmap : NativeObject
{
    public VecI Size
    {
        get => DrawingBackendApi.Current.BitmapImplementation.GetSize(ObjectPointer);
    }
    public Bitmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native => DrawingBackendApi.Current.BitmapImplementation.GetNativeBitmap(ObjectPointer);
    public byte[] Bytes => DrawingBackendApi.Current.BitmapImplementation.GetBytes(ObjectPointer);
    public ImageInfo Info => DrawingBackendApi.Current.BitmapImplementation.GetInfo(ObjectPointer);

    public override void Dispose()
    {
        DrawingBackendApi.Current.BitmapImplementation.Dispose(ObjectPointer);
    }

    public static Bitmap Decode(ReadOnlySpan<byte> buffer)
    {
        return DrawingBackendApi.Current.BitmapImplementation.Decode(buffer);
    }

    public static Bitmap FromImage(Image snapshot)
    {
        return DrawingBackendApi.Current.BitmapImplementation.FromImage(snapshot.ObjectPointer);
    }

    public Pixmap? PeekPixels()
    {
        return DrawingBackendApi.Current.BitmapImplementation.PeekPixels(ObjectPointer);
    }
}
