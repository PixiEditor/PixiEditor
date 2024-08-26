using System;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IColorSpaceImplementation
{
    public ColorSpace CreateSrgb();
    public void Dispose(IntPtr objectPointer);
    public object GetNativeColorSpace(IntPtr objectPointer);
}
