using System;

namespace PixiEditor.DrawingApi.Core.Surface;

public abstract class NativeObject : IDisposable
{
    public IntPtr ObjectPointer { get; protected set; }
    public abstract void Dispose();

    internal NativeObject(IntPtr objPtr)
    {
        ObjectPointer = objPtr;
    }
}
