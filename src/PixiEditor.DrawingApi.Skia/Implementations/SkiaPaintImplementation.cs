using System;
using System.Collections.Generic;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaPaintImplementation : SkObjectImplementation<SKPaint>, IPaintImplementation
    {
        public IntPtr CreatePaint()
        {
            SKPaint paint = new SKPaint();
            ManagedInstances[paint.Handle] = paint;
            return paint.Handle;
        }

        public void Dispose(IntPtr paintObjPointer)
        {
            throw new NotImplementedException();
        }

        public Paint Clone(IntPtr paintObjPointer)
        {
            SKPaint clone = ManagedInstances[paintObjPointer].Clone();
            return new Paint(clone.Handle);
        }
    }
}
