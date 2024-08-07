using System.Collections.Generic;
using System.Text;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders;

public class ShaderBuilder
{
    public Uniforms Uniforms { get; } = new Uniforms();

    public Shader BuildShader()
    {
        string generatedSksl = ToSkSl();
        return new Shader(generatedSksl, Uniforms);
    }

    public string ToSkSl()
    {
        StringBuilder sb = new StringBuilder();
        AppendUniforms(sb);
        sb.AppendLine("half4 main(float2 p)");
        sb.AppendLine("{");
        sb.AppendLine("return original.eval(p).rgba;");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private void AppendUniforms(StringBuilder sb)
    {
        foreach (var uniform in Uniforms)
        {
            sb.AppendLine($"uniform {uniform.Value.UniformName} {uniform.Value.Name};");
        }
    }

    public void WithTexture(string name, Texture texture)
    {
        Uniforms[name] = new Uniform(name, texture.DrawingSurface.Snapshot().ToShader());
    }

    public void WithFloat(string name, float value)
    {
        Uniforms[name] = new Uniform(name, value);
    }

    public void WithVecD(string name, VecD vector)
    {
        Uniforms[name] = new Uniform(name, vector);
    }
}
