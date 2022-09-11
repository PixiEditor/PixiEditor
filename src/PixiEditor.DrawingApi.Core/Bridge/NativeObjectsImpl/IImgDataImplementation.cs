using System;
using System.IO;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IImgDataImplementation
{
    public void Dispose(IntPtr objectPointer);
    public void SaveTo(ImgData imgData, FileStream stream);
    public Stream AsStream(ImgData imgData);
}
