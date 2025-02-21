using Drawie.Backend.Core.Shaders;
using Drawie.Skia.Implementations;

namespace PixiEditor.Backend.Tests;

public class ShaderTests
{
    [Fact]
    public void TestThatFindUniformTypeSolvesTrivialCase()
    {
        string sksl = """
                      uniform float flUniform;

                      half4 main(vec2 coords)
                      {
                          return half4(1, 1, 1, 1);
                      }

                      """;
        UniformValueType? valueType = SkiaShaderImplementation.FindUniformType(sksl, "flUniform");

        Assert.Equal(UniformValueType.Float, valueType);
    }

    [Fact]
    public void TestThatFindUniformTypeSolvesDoesntGetFooledByComment()
    {
        string sksl = """
                      /* uniform vec2 flUniform */uniform float flUniform;
                      // uniform half4 vecUniform
                      uniform vec2 vecUniform;

                      half4 main(vec2 coords)
                      {
                          return half4(1, 1, 1, 1);
                      }

                      """;

        UniformValueType? flValueType = SkiaShaderImplementation.FindUniformType(sksl, "flUniform");
        UniformValueType? vecValueType = SkiaShaderImplementation.FindUniformType(sksl, "vecUniform");

        Assert.Equal(UniformValueType.Float, flValueType);
        Assert.Equal(UniformValueType.Vector2, vecValueType);
    }

    [Fact]
    public void TestThatFindUniformTypeFindsUniformsInTheMiddle()
    {
        string sksl = """
                      float getBlue()
                      {
                       return 0.5;
                      }
                      
                      uniform vec2 uni1;
                      
                      half4 main(vec2 coords)
                      {
                          return half4(uni1.xy, getBlue(), 1);
                      }
                      
                      """;

        UniformValueType? uni1Type = SkiaShaderImplementation.FindUniformType(sksl, "uni1");

        Assert.Equal(UniformValueType.Vector2, uni1Type);
    }

    [Fact]
    public void TestThatMinifiedSkslIsParsedCorrectly()
    {
        string sksl = "uniform float flUniform; half4 main(vec2 coords) { return half4(1, 1, 1, 1); }";
        UniformValueType? valueType = SkiaShaderImplementation.FindUniformType(sksl, "flUniform");

        Assert.Equal(UniformValueType.Float, valueType);
    }

    [Fact]
    public void TestThatAllTypesAreParsedCorrectly()
    {
        string sksl = """
                      uniform float floatUniform;
                      uniform vec2 vector2Uniform;
                      uniform vec3 vec3dUniform;
                      uniform half4 halfArrayUniform;
                      uniform vec4 vec4Uniform;
                      uniform shader shaderUniform;
                      layout(color) uniform half4 colorUniform;
                      layout ( color ) uniform vec4 colorUniform2;

                      half4 main(vec2 coords)
                      {
                          return half4(1, 1, 1, 1);
                      }

                      """;

        UniformValueType? floatType = SkiaShaderImplementation.FindUniformType(sksl, "floatUniform");
        UniformValueType? vector2Type = SkiaShaderImplementation.FindUniformType(sksl, "vector2Uniform");
        UniformValueType? half3Type = SkiaShaderImplementation.FindUniformType(sksl, "halfArrayUniform");
        UniformValueType? vec3dType = SkiaShaderImplementation.FindUniformType(sksl, "vec3dUniform");
        UniformValueType? vec4Type = SkiaShaderImplementation.FindUniformType(sksl, "vec4Uniform");
        UniformValueType? shaderType = SkiaShaderImplementation.FindUniformType(sksl, "shaderUniform");
        UniformValueType? colorType = SkiaShaderImplementation.FindUniformType(sksl, "colorUniform");
        UniformValueType? colorType2 = SkiaShaderImplementation.FindUniformType(sksl, "colorUniform2");

        Assert.Equal(UniformValueType.Float, floatType);
        Assert.Equal(UniformValueType.Vector2, vector2Type);
        Assert.Equal(UniformValueType.Vector3, vec3dType);
        Assert.Equal(UniformValueType.Vector4, half3Type);
        Assert.Equal(UniformValueType.Vector4, vec4Type);
        Assert.Equal(UniformValueType.Shader, shaderType);
        Assert.Equal(UniformValueType.Color, colorType);
        Assert.Equal(UniformValueType.Color, colorType2);
    }
}