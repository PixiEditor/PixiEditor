using System;
using PixiEditor.DrawingApi.Core.Bridge;

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
}
