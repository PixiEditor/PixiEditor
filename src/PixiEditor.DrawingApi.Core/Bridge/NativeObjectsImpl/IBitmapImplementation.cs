using System;
using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IBitmapImplementation
{
    public void Dispose(IntPtr objectPointer);
    public Bitmap Decode(ReadOnlySpan<byte> buffer);
}
