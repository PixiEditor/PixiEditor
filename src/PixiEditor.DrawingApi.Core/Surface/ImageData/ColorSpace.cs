using System;
using PixiEditor.DrawingApi.Core.Bridge;

namespace PixiEditor.DrawingApi.Core.Surface.ImageData;

public class ColorSpace : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ColorSpaceImplementation.GetNativeColorSpace(ObjectPointer);

    public ColorSpace(IntPtr objPtr) : base(objPtr)
    {
        
    }
    
    public static ColorSpace CreateSrgb()
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.CreateSrgb();
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorSpaceImplementation.Dispose(ObjectPointer);
    }
}
