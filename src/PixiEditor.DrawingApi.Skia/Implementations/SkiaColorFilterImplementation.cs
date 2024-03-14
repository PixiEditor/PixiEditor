using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaColorFilterImplementation : SkObjectImplementation<SKColorFilter>, IColorFilterImplementation
    {
        public IntPtr CreateBlendMode(Color color, BlendMode blendMode)
        {
            SKColorFilter skColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), (SKBlendMode)blendMode);
            ManagedInstances[skColorFilter.Handle] = skColorFilter;

            return skColorFilter.Handle;
        }

        public void Dispose(ColorFilter colorFilter)
        {
            SKColorFilter skColorFilter = ManagedInstances[colorFilter.ObjectPointer];
            skColorFilter.Dispose();
            ManagedInstances.TryRemove(skColorFilter.Handle, out _);
        }

        public object GetNativeColorFilter(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
