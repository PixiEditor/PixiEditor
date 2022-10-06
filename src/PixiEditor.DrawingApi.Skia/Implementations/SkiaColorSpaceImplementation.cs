using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaColorSpaceImplementation : SkObjectImplementation<SKColorSpace>, IColorSpaceImplementation
    {
        public ColorSpace CreateSrgb()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgb();
            ManagedInstances[skColorSpace.Handle] = skColorSpace;
            return new ColorSpace(skColorSpace.Handle);
        }

        public void Dispose(IntPtr objectPointer)
        {
            ManagedInstances[objectPointer].Dispose();
            
            ManagedInstances.Remove(objectPointer);
        }
    }
}
