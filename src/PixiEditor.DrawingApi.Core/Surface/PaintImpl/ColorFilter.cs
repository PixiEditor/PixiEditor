using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Surface.PaintImpl;

public class ColorFilter : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ColorFilterImplementation.GetNativeColorFilter(ObjectPointer);
    public ColorFilter(IntPtr objPtr) : base(objPtr)
    {
        
    }

    public static ColorFilter CreateBlendMode(Color color, BlendMode blendMode)
    {
        ColorFilter filter = new ColorFilter(DrawingBackendApi.Current.ColorFilterImplementation.CreateBlendMode(color, blendMode));
        return filter;
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorFilterImplementation.Dispose(this);
    }
}
