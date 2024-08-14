using System;
using System.ComponentModel;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders;

public struct Uniform
{
    public string Name { get; }
    public float FloatValue { get; }
    public float[] FloatArrayValue { get; }
    public Shader ShaderValue { get; }
    public string UniformName { get; }

    public string LayoutOf { get; } = string.Empty;

    public UniformValueType DataType { get; }

    public Uniform(string name, float value)
    {
        Name = name;
        FloatValue = value;
        FloatArrayValue = default;
        ShaderValue = default;
        DataType = UniformValueType.Float;
        UniformName = "float";
    }
    
    public Uniform(string name, VecD vector)
    {
        Name = name;
        FloatValue = default;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y };
        ShaderValue = default;
        DataType = UniformValueType.FloatArray;
        UniformName = "float2";
    }
    
    public Uniform(string name, Shader value)
    {
        Name = name;
        FloatValue = default;
        FloatArrayValue = default;
        ShaderValue = value;
        DataType = UniformValueType.Shader;
        UniformName = "shader";
    }
    
    public Uniform(string name, Color color)
    {
        Name = name;
        FloatValue = default;
        FloatArrayValue = new float[] { color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f };
        ShaderValue = default;
        DataType = UniformValueType.FloatArray;
        //LayoutOf = "color"; TODO: Doesn't work with SkiaSharp 2.8x, check with 3.0 when it releases
        UniformName = "half4";
    }

    public void Dispose()
    {
        ShaderValue?.Dispose();
    }
}

public enum UniformValueType
{
    Float,
    FloatArray,
    Shader
}
