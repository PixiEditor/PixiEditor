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
        private SkObjectImplementation<SKSurface>? _surfaceImplementation;
        
        public SkiaImageImplementation(SkObjectImplementation<SKData> imgDataImplementation)
        {
            _imgImplementation = imgDataImplementation;
        }
        
        public void SetSurfaceImplementation(SkObjectImplementation<SKSurface> surfaceImplementation)
        {
            _surfaceImplementation = surfaceImplementation;
        }
        
        public Image Snapshot(DrawingSurface drawingSurface)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];
            SKImage snapshot = surface.Snapshot();
            
            ManagedInstances[snapshot.Handle] = snapshot;
            return new Image(snapshot.Handle);
        }
        
        public Image? FromEncodedData(byte[] dataBytes)
        {
            SKImage img = SKImage.FromEncodedData(dataBytes);
            if (img is null)
                return null;
            ManagedInstances[img.Handle] = img;
            
            return new Image(img.Handle);
        }

        public void DisposeImage(Image image)
        {
            ManagedInstances[image.ObjectPointer].Dispose();
            ManagedInstances.TryRemove(image.ObjectPointer, out _);
        }

        public Image? FromEncodedData(string path)
        {
            var nativeImg = SKImage.FromEncodedData(path);
            if (nativeImg is null)
                return null;
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

        public ImgData Encode(Image image, EncodedImageFormat format, int quality)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var encoded = native.Encode((SKEncodedImageFormat)format, quality);
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
