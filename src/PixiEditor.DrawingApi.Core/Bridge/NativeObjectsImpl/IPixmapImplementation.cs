using System;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IPixmapImplementation
{
    public void Dispose(IntPtr objectPointer);
    public IntPtr GetPixels(IntPtr objectPointer);
}
