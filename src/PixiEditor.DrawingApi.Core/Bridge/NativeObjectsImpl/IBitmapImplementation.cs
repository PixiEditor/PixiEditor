using System;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IBitmapImplementation
{
    public void Dispose(IntPtr objectPointer);
    public Bitmap Decode(ReadOnlySpan<byte> buffer);
    public object GetNativeBitmap(IntPtr objectPointer);
    public Bitmap FromImage(IntPtr snapshot);
}
