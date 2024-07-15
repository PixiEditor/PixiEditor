using System;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IShaderImplementation
{
    public IntPtr CreateShader();
    public void Dispose(IntPtr shaderObjPointer);
    public Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors);
    public Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors);
    public object GetNativeShader(IntPtr objectPointer);
}
