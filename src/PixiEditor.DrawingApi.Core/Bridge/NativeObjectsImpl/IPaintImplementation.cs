using System;
using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl
{
    public interface IPaintImplementation
    {
        public IntPtr CreatePaint();
        public void Dispose(IntPtr paintObjPointer);
        public Paint Clone(IntPtr paintObjPointer);
    }
}
