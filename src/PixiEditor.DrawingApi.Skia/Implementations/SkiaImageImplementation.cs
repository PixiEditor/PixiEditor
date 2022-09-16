using System;
using System.Collections.Generic;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaImageImplementation : IImageImplementation
    {
        internal readonly Dictionary<IntPtr, SKImage> ManagedImages = new Dictionary<IntPtr, SKImage>();

        private readonly SkiaImgDataImplementation _imgImplementation;
        
        public SkiaImageImplementation(SkiaImgDataImplementation imgDataImplementation)
        {
            _imgImplementation = imgDataImplementation;
        }
        
        public Image Snapshot(DrawingSurface drawingSurface)
        {
            throw new NotImplementedException();
        }

        public void DisposeImage(Image image)
        {
            ManagedImages[image.ObjectPointer].Dispose();
            ManagedImages.Remove(image.ObjectPointer);
        }

        public Image FromEncodedData(string path)
        {
            var nativeImg = SKImage.FromEncodedData(path);
            ManagedImages[nativeImg.Handle] = nativeImg;
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
            var native = ManagedImages[image.ObjectPointer];
            var encoded = native.Encode();
            _imgImplementation.ManagedImgDataObjects[encoded.Handle] = encoded;
            return new ImgData(encoded.Handle);
        }

        public int GetWidth(IntPtr objectPointer)
        {
            return ManagedImages[objectPointer].Width;
        }

        public int GetHeight(IntPtr objectPointer)
        {
            return ManagedImages[objectPointer].Height;
        }
    }
}
