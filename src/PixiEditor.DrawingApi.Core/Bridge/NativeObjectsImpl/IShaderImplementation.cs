using System;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IShaderImplementation
{
    public IntPtr CreateShader();
    public void Dispose(IntPtr shaderObjPointer);
    public Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors);
    public Shader? CreateFromSksl(string sksl, bool isOpaque, Uniforms uniforms, out string errors);
    public Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors);
    public Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
    public Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
    public object GetNativeShader(IntPtr objectPointer);
    public Shader WithUpdatedUniforms(IntPtr objectPointer, Uniforms uniforms);
    public void SetLocalMatrix(IntPtr objectPointer, Matrix3X3 matrix);
}
