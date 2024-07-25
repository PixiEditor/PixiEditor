using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaBitmapImplementation : SkObjectImplementation<SKBitmap>, IBitmapImplementation
    {
        public SkiaImageImplementation ImageImplementation { get; }
        public SkiaBitmapImplementation(SkiaImageImplementation imgImpl)
        {
            ImageImplementation = imgImpl;
        }

        public void Dispose(IntPtr objectPointer)
        {
            SKBitmap bitmap = ManagedInstances[objectPointer];
            bitmap.Dispose();   
            
            ManagedInstances.TryRemove(objectPointer, out _);
        }

        public Bitmap Decode(ReadOnlySpan<byte> buffer)
        {
            SKBitmap skBitmap = SKBitmap.Decode(buffer);
            ManagedInstances[skBitmap.Handle] = skBitmap;
            return new Bitmap(skBitmap.Handle);
        }

        public Bitmap FromImage(IntPtr ptr)
        {
            SKImage image = ImageImplementation.ManagedInstances[ptr];
            SKBitmap skBitmap = SKBitmap.FromImage(image);
            ManagedInstances[skBitmap.Handle] = skBitmap;
            return new Bitmap(skBitmap.Handle);
        }
        
        public VecI GetSize(IntPtr objectPointer)
        {
            SKBitmap bitmap = ManagedInstances[objectPointer];
            return new VecI(bitmap.Width, bitmap.Height);
        }

        public object GetNativeBitmap(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
