using System;
using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface IPaintOperations
    {
        public IntPtr CreatePaint();
        public void Dispose(IntPtr paintObjPointer);
    }
}
