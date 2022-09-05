using System;
using PixiEditor.DrawingApi.Core.Bridge;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Core.Surface;

public class Pixmap : NativeObject
{
    internal Pixmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PixmapImplementation.Dispose(ObjectPointer);
    }

    public IntPtr GetPixels()
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixels(ObjectPointer);
    }
}
