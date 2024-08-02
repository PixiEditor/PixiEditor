using System;
using System.IO;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IImgDataImplementation
{
    public void Dispose(IntPtr objectPointer);
    public void SaveTo(ImgData imgData, FileStream stream);
    public Stream AsStream(ImgData imgData);
    public ReadOnlySpan<byte> AsSpan(ImgData imgData);
    public object GetNativeImgData(IntPtr objectPointer);
}
