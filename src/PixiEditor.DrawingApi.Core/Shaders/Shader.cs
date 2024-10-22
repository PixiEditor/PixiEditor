using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Exceptions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders;

public class Shader : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ShaderImplementation.GetNativeShader(ObjectPointer);
    
    public Shader(IntPtr objPtr) : base(objPtr)
    {
    }

    public Shader(string sksl, Uniforms uniforms) : base(DrawingBackendApi.Current.ShaderImplementation.CreateFromSksl(sksl, false, uniforms, out string errors)?.ObjectPointer ?? IntPtr.Zero)
    {
        if (!string.IsNullOrEmpty(errors))
        {
            throw new ShaderCompilationException(errors, sksl);
        }
    }
    
    /// <summary>
    ///     Creates updated version of shader with new uniforms. THIS FUNCTION DISPOSES OLD SHADER.
    /// </summary>
    /// <param name="uniforms"></param>
    /// <returns></returns>
    public Shader WithUpdatedUniforms(Uniforms uniforms)
    {
        return DrawingBackendApi.Current.ShaderImplementation.WithUpdatedUniforms(ObjectPointer, uniforms);
    }

    public static Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors)
    {
       return DrawingBackendApi.Current.ShaderImplementation.CreateFromSksl(sksl, isOpaque, out errors);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ShaderImplementation.Dispose(ObjectPointer);
    }

    public static Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors);
    }

    public static Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinNoiseTurbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed);
    }

    public static Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinFractalNoise(baseFrequencyX, baseFrequencyY, numOctaves, seed);
    }

    public void SetLocalMatrix(Matrix3X3 matrix)
    {
        DrawingBackendApi.Current.ShaderImplementation.SetLocalMatrix(ObjectPointer, matrix);
    }

    public static Shader CreateBitmap(Bitmap bitmap, ShaderTileMode tileModeX, ShaderTileMode tileModeY, Matrix3X3 matrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateBitmap(bitmap, tileModeX, tileModeY, matrix);
    }
}
