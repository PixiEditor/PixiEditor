using System;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IColorSpaceImplementation
{
    public ColorSpace CreateSrgb();
    public void Dispose(IntPtr objectPointer);
}
