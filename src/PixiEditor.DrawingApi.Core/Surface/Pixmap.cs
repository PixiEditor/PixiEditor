using System;
using PixiEditor.DrawingApi.Core.Bridge;

namespace PixiEditor.DrawingApi.Core.Surface;

public class Pixmap : NativeObject
{
    internal Pixmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PixmapImplementation.Dispose(ObjectPointer);
    }

    public IntPtr GetPixels()
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixels(ObjectPointer);
    }
}
