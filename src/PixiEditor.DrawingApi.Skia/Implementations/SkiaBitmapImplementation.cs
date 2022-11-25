using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaBitmapImplementation : SkObjectImplementation<SKBitmap>, IBitmapImplementation
    {
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
    }
}
