using System;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface IImageImplementation
    {
        public Image Snapshot(DrawingSurface drawingSurface);
        public Image Snapshot(DrawingSurface drawingSurface, RectI bounds);
        public void DisposeImage(Image image);
        public Image? FromEncodedData(string path);
        public Image? FromEncodedData(byte[] dataBytes);
        public void GetColorShifts(ref int platformColorAlphaShift, ref int platformColorRedShift, ref int platformColorGreenShift, ref int platformColorBlueShift);
        public ImgData Encode(Image image);
        public ImgData Encode(Image image, EncodedImageFormat format, int quality);
        public int GetWidth(IntPtr objectPointer);
        public int GetHeight(IntPtr objectPointer);
        public object GetNativeImage(IntPtr objectPointer);
    }
}
