using System;
using System.Collections.Generic;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaImageImplementation : SkObjectImplementation<SKImage>, IImageImplementation
    {
        private readonly SkObjectImplementation<SKData> _imgImplementation;
        
        public SkiaImageImplementation(SkObjectImplementation<SKData> imgDataImplementation)
        {
            _imgImplementation = imgDataImplementation;
        }
        
        public Image Snapshot(DrawingSurface drawingSurface)
        {
            throw new NotImplementedException();
        }

        public void DisposeImage(Image image)
        {
            ManagedInstances[image.ObjectPointer].Dispose();
            ManagedInstances.Remove(image.ObjectPointer);
        }

        public Image FromEncodedData(string path)
        {
            var nativeImg = SKImage.FromEncodedData(path);
            ManagedInstances[nativeImg.Handle] = nativeImg;
            return new Image(nativeImg.Handle);
        }

        public void GetColorShifts(ref int platformColorAlphaShift, ref int platformColorRedShift, ref int platformColorGreenShift,
            ref int platformColorBlueShift)
        {
            platformColorAlphaShift = SKImageInfo.PlatformColorAlphaShift;
            platformColorRedShift = SKImageInfo.PlatformColorRedShift;
            platformColorGreenShift = SKImageInfo.PlatformColorGreenShift;
            platformColorBlueShift = SKImageInfo.PlatformColorBlueShift;
        }

        public ImgData Encode(Image image)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var encoded = native.Encode();
            _imgImplementation.ManagedInstances[encoded.Handle] = encoded;
            return new ImgData(encoded.Handle);
        }

        public int GetWidth(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].Width;
        }

        public int GetHeight(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].Height;
        }
    }
}
