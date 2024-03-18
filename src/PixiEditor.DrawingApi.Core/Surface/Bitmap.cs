using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Surface;

public class Bitmap : NativeObject
{
    public Bitmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native => DrawingBackendApi.Current.BitmapImplementation.GetNativeBitmap(ObjectPointer);

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
}
