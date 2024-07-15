using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surface.PaintImpl;

public class Shader : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ShaderImplementation.GetNativeShader(ObjectPointer);
    
    public Shader(IntPtr objPtr) : base(objPtr)
    {
    }
    
    public static Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors)
    {
       return DrawingBackendApi.Current.ShaderImplementation.CreateFromSksl(sksl, isOpaque, out errors);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PaintImplementation.Dispose(ObjectPointer);
    }

    public static Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors);
    }
}
